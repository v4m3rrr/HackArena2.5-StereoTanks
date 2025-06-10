#pragma once

#include "../processed-packets.h"
#include "utils.h"


class Bot {
 public:
    /// DO NOT DELETE
    /// time in milliseconds after which the NextMove() answer is not sent to server, CAN BE CHANGED WHENEVER YOU WANT
    int skipResponse = 99;
	std::string myId;
    std::string teamName;

	Bot();
	void Init(const LobbyData& lobbyData);
	ResponseVariant NextMove(const GameState& gameState);
	void OnGameEnded(const EndGameLobby& endGameLobby);
    void OnWarningReceived(WarningType warningType, std::optional<std::string>& message);
    void OnGameStarting();

    LobbyData lobbyData;
    int dim;
    KnowledgeMap knowledgeMap;
    std::vector<std::vector<bool>> isWall;
	std::vector<std::vector<int>> wallType;
    std::vector<std::vector<char>> zoneName;

    int myTankIdx = -1;

    float myShares;
    float oppShares;
    float neutralShares;

    class TankState {
    public:
        Bot& bot; 
        std::string myId;
    
        Tank myTank;
        OrientedPosition myPos;
        OrientedPosition lastPos;
        Direction myTurretDir;
        int myBulletCount = 3;
    
        TankState(Bot& _bot) : bot(_bot) {}
    
        std::optional<ResponseVariant> NextMove(const GameState& gameState);
        bool canSeeEnemy(const GameState& gameState) const;
        bool canSeeLowHpAlly(const GameState& gameState) const;
        bool canSeeTank(const GameState& gameState, bool enemy) const;

        bool willFireHitForSure(const GameState& gameState) const;
        bool willBeHitByBullet(const GameState& gameState, const OrientedPosition& pos) const;
        void initMyTankHelper(const GameState& gameState);
        void initMyTank(const GameState& gameState);

        bool canMoveForwardInsideZone(const OrientedPosition& pos) const;
        bool canMoveBackwardInsideZone(const OrientedPosition& pos) const;

        ResponseVariant BeDrunkInsideZone(const GameState& gameState);

        std::optional<ResponseVariant> shootIfSeeingEnemy(
            const GameState& gameState, 
            bool useLaserIfPossible = true, 
            bool useDoubleBulletIfPossible = true
        );
    
        std::optional<ResponseVariant> shootIfWillFireHitForSure(
            const GameState& gameState, 
            bool useLaserIfPossible = true, 
            bool useDoubleBulletIfPossible = true
        );
        
        std::optional<ResponseVariant> healIfSeeingAlly(
            const GameState& gameState
        );
    
        std::optional<ResponseVariant> dropMineIfReasonable(const GameState& gameState);
        std::optional<ResponseVariant> useRadarIfPossible(const GameState& gameState);

        std::optional<ResponseVariant> rotateToEnemy(const GameState& gameState);
        std::optional<ResponseVariant> dodgeIfNoAmmoAndWillBeHit(const GameState& gameState);

        template<class F>
        std::optional<ResponseVariant> shootIf(
                const GameState& gameState, 
                F&& f, 
                bool useLaserIfPossible = true, 
                bool useDoubleBulletIfPossible = true
        ) {

            if (!canShootLaser(myTank)) {
                useLaserIfPossible = false;
            }
            if (!canShootDouble(myTank)) {
                useDoubleBulletIfPossible = false;
            }

            bool ignoreBulletCount = useDoubleBulletIfPossible | useLaserIfPossible;
    
            if (myBulletCount == 0 && !ignoreBulletCount) {
                return std::nullopt;
            }
    
            if (!f(gameState)) {
                return std::nullopt;
            }
    
            if (useLaserIfPossible) {
                return AbilityUse{AbilityType::useLaser};
            }
    
            if (useDoubleBulletIfPossible) {
                return AbilityUse{AbilityType::fireDoubleBullet};
            }
            
            return AbilityUse{AbilityType::fireBullet};
        }

        template<class F>
        std::optional<ResponseVariant> bfsStrategy(const GameState& gameState, F&& f) {
            auto result = bot.bfs(myPos, f);
            if (!result) {
                return std::nullopt;
            }

            auto nxtMove = result->move;
            auto eta = result->eta;
            auto nextPos = afterMove(myPos, nxtMove);

            bool hitNow = willBeHitByBullet(gameState, myPos);
            bool hitNxt = willBeHitByBullet(gameState, nextPos);

            if (hitNxt && !hitNow) {
                // TODO: wait or think about rotating?
                return Wait{};
            }

            if (std::holds_alternative<MoveDirection>(nxtMove)) {
                return Move{std::get<MoveDirection>(nxtMove)};
            } else {
                return Rotate{std::get<RotationDirection>(nxtMove), RotationDirection::none};
            }
        }
    };
    
    TankState tankState[2] = {TankState(*this), TankState(*this)};

    void onFirstNextMove(const GameState& gameState);
    void initIsWall(const std::vector<std::vector<Tile>>& tiles);
    void initWallType(const std::vector<std::vector<Tile>>& tiles);
    void initZoneName(const std::vector<std::vector<Tile>>& tiles);
    void initMyTanks(const GameState& gameState);
    void initShares(const GameState& gameState);

    float captureProb() const;

    struct BfsResult {
        MoveOrRotation move;
        OrientedPosition finalPos;
        int eta;
    };

    template<class F>
    std::optional<BfsResult> bfs(const OrientedPosition& start, F&& f) {
        std::queue<std::pair<OrientedPosition, int>> q;
        std::vector<std::vector<std::vector<bool>>> visited(dim, std::vector<std::vector<bool>>(dim, std::vector<bool>(4, false)));
        std::vector<std::vector<std::vector<MoveOrRotation>>> from(dim, std::vector<std::vector<MoveOrRotation>>(dim, std::vector<MoveOrRotation>(4))); // i, j, dir
        q.push({start, 0});
        visited[start.pos.x][start.pos.y][getDirId(start.dir)] = true;

        bool found = false;
        OrientedPosition finish;
        int eta = -1;

        while (!q.empty()) {
            auto [pos, timer] = q.front();
            q.pop();

            if (f(pos, timer)) {
                found = true;
                finish = pos;
                eta = timer;
                break;
            }

            for (const MoveOrRotation &move : ALL_ACTIONS) {
                auto nextPos = afterMove(pos, move);
                if (!isValid(nextPos, dim)) {
                    continue;
                }

                int x = nextPos.pos.x;
                int y = nextPos.pos.y;
                int dir = getDirId(nextPos.dir);

                if (wallType[x][y] > 0) {
                    continue;
                }

                if (knowledgeMap.containsMine(nextPos.pos)) {
                    continue;
                }

                if (knowledgeMap.isOnBulletTraj(x, y)) {
                    continue;
                }

                if (visited[x][y][dir]) {
                    continue;
                }

                visited[x][y][dir] = true;
                from[x][y][dir] = reversed(move);
                q.push({nextPos, timer + 1});
            }
        }

        if (!found) {
            return std::nullopt;
        }

        OrientedPosition cur = finish;
        MoveOrRotation lastMove = from[cur.pos.x][cur.pos.y][getDirId(cur.dir)];

        while (cur != start) {
            lastMove = from[cur.pos.x][cur.pos.y][getDirId(cur.dir)];
            cur.move(lastMove);
        }

        lastMove = reversed(lastMove);
        return BfsResult{lastMove, finish, eta};
    }
};
