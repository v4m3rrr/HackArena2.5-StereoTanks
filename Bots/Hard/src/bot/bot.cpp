#include "bot.h"
#include <utility>
#include <random>
#include <iostream>
#include <map>
#include <string>

void Bot::OnWarningReceived(WarningType warningType, std::optional<std::string> &message) {
    std::cout << "Warning received: ";
    switch (warningType) {
        case WarningType::CustomWarning:
            std::cout << "Custom warning";
            if (message.has_value()) {
                std::cout << " - " << message.value();
            }
            break;
        case WarningType::PlayerAlreadyMadeActionWarning:
            std::cout << "Player already made an action";
            break;
        case WarningType::ActionIgnoredDueToDeadWarning:
            std::cout << "Action ignored because tank is dead";
            break;
        case WarningType::SlowResponseWarning:
            std::cout << "Response was too slow";
            break;
        default:
            std::cout << "Unknown warning type";
    }
    std::cout << std::endl;
}

void Bot::OnGameStarting() {
    std::cout << "Game is starting!" << std::endl;
}

void Bot::OnGameEnded(const EndGameLobby& endGameLobby) {
    std::cout << "Game has ended! Final scores:" << std::endl;
    for (const auto& team : endGameLobby.teams) {
        std::cout << "Team " << team.name << ": " << team.score << " points" << std::endl;
        std::cout << "  Players:" << std::endl;
        for (const auto& player : team.players) {
            std::cout << "    - " << player.id
                      << " (Tank type: " << (player.tankType == TankType::Light ? "Light" : "Heavy")
                      << ", Kills: " << player.kills << ")" << std::endl;
        }
    }
    std::cout << "Thank you for playing!" << std::endl;
}

Bot::Bot() = default;

void Bot::Init(const LobbyData& _lobbyData) {
    lobbyData = _lobbyData;
    dim = lobbyData.gridDimension;
    myId = lobbyData.myId;
    teamName = lobbyData.teamName;
    skipResponse = lobbyData.broadcastInterval - 1;
    knowledgeMap.init(dim);

    // Print basic information about the game
    std::cout << "Bot initialized with ID: " << myId << std::endl;
    std::cout << "Team name: " << lobbyData.teamName << std::endl;
    std::cout << "Grid dimension: " << lobbyData.gridDimension << std::endl;
    std::cout << "Number of players: " << lobbyData.numberOfPlayers << std::endl;
    // Print team information
    std::cout << "Teams in game:" << std::endl;
    for (const auto& team : lobbyData.teams) {
        std::cout << "  Team: " << team.name << " (Players: " << team.players.size() << ")" << std::endl;
    }
}

void Bot::initIsWall(const std::vector<std::vector<Tile>>& tiles) {
    isWall = std::vector<std::vector<bool>>(dim, std::vector<bool>(dim, false));
    for (int i = 0; i < dim; ++i) {
        for (int j = 0; j < dim; ++j) {
            isWall[i][j] = std::any_of(tiles[i][j].objects.begin(), tiles[i][j].objects.end(), [](const TileVariant& object) {
                return std::holds_alternative<Wall>(object);
            });
        }
    }
}

void Bot::initWallType(const std::vector<std::vector<Tile>>& tiles) {
    wallType = std::vector<std::vector<int>>(dim, std::vector<int>(dim, 0));
    for (int i = 0; i < dim; ++i) {
        for (int j = 0; j < dim; ++j) {
            for (const TileVariant& object : tiles[i][j].objects) {
                if (std::holds_alternative<Wall>(object)) {
                    const Wall& wall = std::get<Wall>(object);
                    if (wall.type == WallType::solid) {
                        wallType[i][j] = 2; // Solid wall
                    } else if (wall.type == WallType::penetrable) {
                        wallType[i][j] = 1; // Penetrable wall
                    }
                }
            }
        }
    }
}

void Bot::initZoneName(const std::vector<std::vector<Tile>>& tiles) {
    zoneName = std::vector<std::vector<char>>(dim, std::vector<char>(dim, '?'));
    for (int i = 0; i < dim; ++i) {
        for (int j = 0; j < dim; ++j) {
            zoneName[i][j] = tiles[i][j].zoneName;
        }
    }
}

void Bot::onFirstNextMove(const GameState& gameState) {
    initIsWall(gameState.map.tiles);
    initWallType(gameState.map.tiles);
    // print wallType;
    for (int i = 0; i < dim; ++i) {
        for (int j = 0; j < dim; ++j) {
            std::cout << wallType[i][j] << " ";
        }
        std::cout << std::endl;
    }
    initZoneName(gameState.map.tiles);
}

bool Bot::TankState::canSeeTank(const GameState& gameState, bool enemy) const {
    const int dim = bot.dim;
    int x = myPos.pos.x;
    int y = myPos.pos.y;
    int dir = getDirId(myTurretDir);

    auto [dx, dy] = Position::DIRECTIONS[dir];

    for (int i = 1; i < dim; ++i) {
        x += dx;
        y += dy;
        if (!isValid(Position(x, y), dim)) {
            break;
        }
        if (bot.wallType[x][y] == 2) {
            break;
        }
        if (!bot.knowledgeMap.isVisible[x][y]) {
            break;
        }
        for (const TileVariant& object : gameState.map.tiles[x][y].objects) {
            if (std::holds_alternative<Tank>(object)) {
                const Tank& tank = std::get<Tank>(object);
                if (enemy) {
                    if (isEnemy(tank)) {
                        return true;
                    }
                } else {
                    if (!isEnemy(tank) && tank.ownerId != myId && tank.health != 100) {
                        return true;
                    }
                }
            }
        }
    }

    return false;
}

bool Bot::TankState::canSeeEnemy(const GameState& gameState) const {
    return canSeeTank(gameState, true);
}

bool Bot::TankState::canSeeLowHpAlly(const GameState& gameState) const {
    return canSeeTank(gameState, false);
}

bool Bot::TankState::willBeHitByBullet(const GameState& gameState, const OrientedPosition& pos) const {
    int x = pos.pos.x;
    int y = pos.pos.y;
    return bot.knowledgeMap.willBeHitByBulletInNextMove(x, y);
}

bool Bot::TankState::willFireHitForSure(const GameState& gameState) const {
    const int dim = bot.dim;
    int x = myPos.pos.x;
    int y = myPos.pos.y;
    int dir = getDirId(myTurretDir);

    auto [dx, dy] = Position::DIRECTIONS[dir];

    int bound = 2;

    if (canShootLaser(myTank)) {
        bound = dim;
    }

    for (int i = 1; i < bound; i++) {
        x += dx;
        y += dy;
        if (!isValid(Position(x, y), dim)) {
            break;
        }
        if (bot.wallType[x][y] == 2) {
            break;
        }
        // if (!gameState.map.tiles[x][y].isVisible) {
        if (!bot.knowledgeMap.isVisible[x][y]) {
            break;
        }
        for (const TileVariant& object : gameState.map.tiles[x][y].objects) {
            if (std::holds_alternative<Tank>(object)) {
                const Tank& tank = std::get<Tank>(object);
                if (!isEnemy(tank)) {
                    return false;
                }
                if (isParallel(myTurretDir, tank.direction)) {
                    return true;
                }
            }
        }
    }

    return false;
}

void Bot::TankState::initMyTankHelper(const GameState& gameState) {
    for (int i = 0; i < bot.dim; i++) {
        for (int j = 0; j < bot.dim; j++) {
            for (const TileVariant& object : gameState.map.tiles[i][j].objects) {
                if (std::holds_alternative<Tank>(object)) {
                    const Tank& tank = std::get<Tank>(object);
                    if (tank.ownerId == myId) {
                        myTank = tank;
                        myPos = OrientedPosition{Position(i, j), tank.direction};
                        return;
                    }
                }
            }
        }
    }
}

void Bot::TankState::initMyTank(const GameState& gameState) {
    myId = bot.myId;
    initMyTankHelper(gameState);
    myTurretDir = myTank.turret.direction;
    assert (myTank.turret.bulletCount.has_value());
    myBulletCount = myTank.turret.bulletCount.value();
}

void Bot::initMyTanks(const GameState& gameState) {
    for (int i = 0; i < 2; i++) {
        tankState[i].initMyTank(gameState);
    }

    myTankIdx = -1;
    for (int i = 0; i < 2; i++) {
        if (tankState[i].myId == myId) {
            myTankIdx = i;
            break;
        }
    }

    assert(myTankIdx != -1);
}

std::optional<ResponseVariant> Bot::TankState::NextMove(const GameState& gameState) {
    static std::random_device rd;
    static std::mt19937 gen(rd());
    static std::uniform_real_distribution<float> dist(0, 1);

    std::optional<ResponseVariant> response;
	
	if(gameState.time % 6 != 0){
		return CaptureZone{};
	}
    
    response = shootIfWillFireHitForSure(gameState);
    if (response.has_value()) 
    return response.value();
    
    
    response = shootIfWillFireHitForSure(gameState);    
    if (response.has_value()) {
        std::cout << __LINE__ << ": Fire, sure hit" << std::endl;
        return response.value();
    }
    

    response = healIfSeeingAlly(gameState);
    if (response.has_value())
        return response.value();

    // TODO: jakieś gówno XD niech będzie na razie ale potem można wyjebać
    if (lastPos == myPos && gen() % 16 == 0) {
        std::cout << __LINE__ << ": Gowno" << std::endl;
        response = shootIfSeeingEnemy(gameState);
        if (response.has_value())
        return response.value();
        return BeDrunkInsideZone(gameState);
    }
    
    
    response = dodgeIfNoAmmoAndWillBeHit(gameState);
    if (response.has_value()) {
        std::cout << __LINE__ << ": Dodge" << std::endl;
        return response.value();
    }

    auto bullet = closestBullet(gameState, myPos.pos);
    auto isOnBulletLine = [&](const OrientedPosition& oPos, int timer) {
        return oPos.pos.x != bullet.x && oPos.pos.y != bullet.y;
    };
    
    if (bullet.x != 1e9) {
        response = bfsStrategy(gameState, isOnBulletLine);
        if (response.has_value()) {
            std::cout << __LINE__ << ": Run from closest bullet" << std::endl;
            return response.value();
        }
    }
    
    int TIME_FOR_HEAL = 10;
    
    std::vector<std::pair<Bullet, Position>> bullets = getHealingBullets(gameState);
    std::optional<Tank> teammate = findTeammate(gameState, myId);
    int teammate_health = 0;
    if (teammate.has_value()) {
        teammate_health = teammate.value().health.value_or(0);
    }
    if (bullets.size() > 0 && myTank.health.value_or(0) < teammate_health) {
        auto isHealingBullet = [&](const OrientedPosition& oPos, int timer) {
            if (timer > TIME_FOR_HEAL) {
                return false;
            }
            
            for (auto [bullet, pos] : bullets) {
                for (int i = 0; i < timer; i++) {
                    for (int j = 0; j < bullet.speed; j++) {
                        pos.move(bullet.direction);
                        if (!isValid(pos, bot.dim) || bot.wallType[pos.x][pos.y] == 2) {
                            return false;
                        }
                    }
                }
                if (pos.x == oPos.pos.x && pos.y == oPos.pos.y  && bot.wallType[pos.x][pos.y] == 0) {
                    return true;
                }
            }
            
            return false;
        };
    
        response = bfsStrategy(gameState, isHealingBullet);
        if (response.has_value()) {
            return response.value();
        }
    }
    
    response = dropMineIfReasonable(gameState);
    if (response.has_value()) {
        std::cout << __LINE__ << ": Drop mine" << std::endl;
        return response.value();
    }

    response = useRadarIfPossible(gameState);
    if (response.has_value()) {
        std::cout << __LINE__ << ": Use radar" << std::endl;
        return response.value();
    }


    auto isZone = targetZone(bot.zoneName);
    if (isZone(myPos, 0)) {
        std::cout << __LINE__ << ": Inside zone" << std::endl;
        float captureProb = bot.captureProb() * 0.8;
        if (captureProb >= dist(gen)) {
            std::cout << __LINE__ << ": Capturing (" << captureProb << ")" << std::endl;
            return CaptureZone{};
        }
        else {
            std::cout << __LINE__ << ": Not capturing (" << captureProb << ")" << std::endl;
            response = shootIfSeeingEnemy(gameState, false, false);
            if (response.has_value()) 
                return response.value();
        
            response = rotateToEnemy(gameState);
            if (response.has_value()) 
                return response.value();

            if (gen() % 4 == 0) {
                return BeDrunkInsideZone(gameState);
            }

            return CaptureZone{};
        }
    }
    
    response = bfsStrategy(gameState, isZone);
    if (response.has_value()) {
        std::cout << __LINE__ << ": Going to zone" << std::endl;
        return response.value();
    }
    
    // TODO: dziwny przypadek, jestśmy odizolowanie od strefy, można przemyśleć co z tym zrobić
    std::cout << __LINE__ << ": Deadlock, random" << std::endl;
    return BeDrunkInsideZone(gameState);
}

ResponseVariant Bot::NextMove(const GameState& gameState) {
    // // std::cout << "Chuj " << gameState.time << ":" << std::endl;

    if (gameState.time == 1) {
        onFirstNextMove(gameState);
    }

    for (int i = 0; i < 2; i++) {
        tankState[i].lastPos = tankState[i].myPos;
    }

    initMyTanks(gameState);
    initShares(gameState);
    knowledgeMap.update(gameState);

    std::optional<ResponseVariant> response;
    
    // mój czołg niezależnie robi ruch od reszty świata
    response = tankState[myTankIdx].NextMove(gameState);

    // na razie forcujemy żeby się nie komunikowali i najwyżej losowy ruch robili
    assert(response.has_value());

    return response.value();
}


std::optional<ResponseVariant> Bot::TankState::shootIfSeeingEnemy(
    const GameState& gameState, 
    bool useLaserIfPossible, 
    bool useDoubleBulletIfPossible
) {
    return shootIf(gameState, [&](const GameState& gameState) {
        return canSeeEnemy(gameState);
    }, useLaserIfPossible, useDoubleBulletIfPossible);
}

std::optional<ResponseVariant> Bot::TankState::healIfSeeingAlly(const GameState& gameState) {
    
    if (canShootHealing(myTank) && canSeeLowHpAlly(gameState)) {
        std::cout << "healing!" << std::endl;
            return AbilityUse{AbilityType::fireHealingBullet};
    }
    return std::nullopt;
}

std::optional<ResponseVariant> Bot::TankState::shootIfWillFireHitForSure(
    const GameState& gameState, 
    bool useLaserIfPossible, 
    bool useDoubleBulletIfPossible
) {
    return shootIf(gameState, [&](const GameState& gameState) {
        return willFireHitForSure(gameState);
    }, useLaserIfPossible, useDoubleBulletIfPossible);
}

std::optional<ResponseVariant> Bot::TankState::rotateToEnemy(const GameState& gameState) {
    auto isVisibleEnemy = [&](const OrientedPosition& pos, int timer) {
        if (!bot.knowledgeMap.isVisible[pos.pos.x][pos.pos.y])
            return false;

        for (const TileVariant& object : gameState.map.tiles[pos.pos.x][pos.pos.y].objects) {
            if (std::holds_alternative<Tank>(object)) {
                const Tank& tank = std::get<Tank>(object);
                if (isEnemy(tank)) {
                    return true;
                }
            }
        }

        return false;
    };

    auto isPotentialEnemy = [&](const OrientedPosition& pos, int timer) {
        if (bot.knowledgeMap.isVisible[pos.pos.x][pos.pos.y])
            return false;

        for (const KnowledgeTileVariant& object : bot.knowledgeMap.tiles[pos.pos.x][pos.pos.y].objects) {
            if (std::holds_alternative<Tank>(object.object)) {
                const Tank& tank = std::get<Tank>(object.object);
                if (isEnemy(tank)) {
                    return true;
                }
            }
        }

        return false;
    };

    auto result = bot.bfs(myPos, isVisibleEnemy);
    if (!result.has_value()) {
        result = bot.bfs(myPos, isPotentialEnemy);
        if (!result.has_value()) {
            return std::nullopt;
        }
    }

    auto enemyPos = result.value().finalPos;

    auto dx = enemyPos.pos.x - myPos.pos.x;
    auto dy = enemyPos.pos.y - myPos.pos.y;

    Direction desiredTankDir;
    Direction desiredTurretDir;

    if (dx >= abs(dy)) {
        desiredTurretDir = Direction::down;
        if (dy >= 0) {
            desiredTankDir = Direction::right;
        } else {
            desiredTankDir = Direction::left;
        }
    } else if (dx <= -abs(dy)) {
        desiredTurretDir = Direction::up;
        if (dy >= 0) {
            desiredTankDir = Direction::right;
        } else {
            desiredTankDir = Direction::left;
        }
    } else if (dy >= abs(dx)) {
        desiredTurretDir = Direction::right;
        if (dx >= 0) {
            desiredTankDir = Direction::down;
        } else {
            desiredTankDir = Direction::up;
        }
    } else if (dy <= -abs(dx)) {
        desiredTurretDir = Direction::left;
        if (dx >= 0) {
            desiredTankDir = Direction::down;
        } else {
            desiredTankDir = Direction::up;
        }
    }

    auto tankRot = getRotationTo(myPos.dir, desiredTankDir);
    auto turretRot = getRotationTo(myTurretDir, desiredTurretDir);

    if (tankRot == RotationDirection::none && turretRot == RotationDirection::none) {
        std::random_device rd;
        std::mt19937 gen(rd());

        if (gen() % 4 != 0) {
            auto nxtPos = afterMove(myPos, MoveDirection::forward);
            if (isValid(nxtPos.pos, bot.dim) && !bot.isWall[nxtPos.pos.x][nxtPos.pos.y]) {
                return Move{MoveDirection::forward};
            }
            return BeDrunkInsideZone(gameState);
        }

        auto nxtPos = afterMove(myPos, MoveDirection::backward);
        switch (gen() % 3) {
            case 0:
                if (isValid(nxtPos.pos, bot.dim) && !bot.isWall[nxtPos.pos.x][nxtPos.pos.y]) {
                    return Move{MoveDirection::backward};
                }
                return BeDrunkInsideZone(gameState);
            case 1:
                return Wait{};
            case 2:
                return BeDrunkInsideZone(gameState);
        }
    }

    return Rotate{tankRot, turretRot};
}

ResponseVariant Bot::TankState::BeDrunkInsideZone(const GameState& gameState) {
    std::random_device rd;
    std::mt19937 gen(rd());

    if (gen() % 2 == 0) {
        bool canMoveForward = canMoveForwardInsideZone(myPos);
        bool canMoveBackward = canMoveBackwardInsideZone(myPos);
        if (canMoveForward && canMoveBackward) {
            auto randomMove = static_cast<MoveDirection>(gen() % 2);
            return Move{randomMove};
        } else if (canMoveForward) {
            return Move{MoveDirection::forward};
        } else if (canMoveBackward) {
            return Move{MoveDirection::backward};
        }
    }

    auto randomRotation1 = static_cast<RotationDirection>(gen() % 2);
    auto randomRotation2 = static_cast<RotationDirection>(gen() % 2);
    return Rotate{randomRotation1, randomRotation2};
}

bool Bot::TankState::canMoveForwardInsideZone(const OrientedPosition& pos) const {
    auto [dx, dy] = Position::DIRECTIONS[getDirId(pos.dir)];
    auto nextPos = pos;
    nextPos.pos.x += dx;
    nextPos.pos.y += dy;
    return isValid(nextPos.pos, bot.dim) && !bot.isWall[nextPos.pos.x][nextPos.pos.y] && bot.zoneName[nextPos.pos.x][nextPos.pos.y] != '?';
}

bool Bot::TankState::canMoveBackwardInsideZone(const OrientedPosition& pos) const {
    auto [dx, dy] = Position::DIRECTIONS[getDirId(pos.dir)];
    auto nextPos = pos;
    nextPos.pos.x -= dx;
    nextPos.pos.y -= dy;
    return isValid(nextPos.pos, bot.dim) && !bot.isWall[nextPos.pos.x][nextPos.pos.y] && bot.zoneName[nextPos.pos.x][nextPos.pos.y] != '?';
}

std::optional<ResponseVariant> Bot::TankState::dropMineIfReasonable(const GameState& gameState) {
    if (canDropMine(myTank) && (isBetweenWalls(myPos.pos, bot.isWall, bot.dim) || bot.zoneName[myPos.pos.x][myPos.pos.y] != '?')) {
        auto [dx, dy] = Position::DIRECTIONS[getDirId(myPos.dir)];

        Position minePos = myPos.pos;
        minePos.x -= dx;
        minePos.y -= dy;

        if (!isValid(minePos, bot.dim) || bot.isWall[minePos.x][minePos.y]) {
            return std::nullopt;
        }

        bot.knowledgeMap.notifyMine(gameState, minePos);

        return AbilityUse{AbilityType::dropMine};
    }

    return std::nullopt;
}

std::optional<ResponseVariant> Bot::TankState::useRadarIfPossible(const GameState& gameState) {
    if (canUseRadar(myTank)) {
        return AbilityUse{AbilityType::useRadar};
    }

    return std::nullopt;
}

std::optional<ResponseVariant> Bot::TankState::dodgeIfNoAmmoAndWillBeHit(const GameState& gameState) {
    for (int i = 0; i < 4; i++) {
        auto [dx, dy] = Position::DIRECTIONS[i];

        if (i == getDirId(myTurretDir)) {
            if (myBulletCount > 0) {
                continue;
            }
            if (canShootLaser(myTank)) {
                continue;
            }
            if (canShootDouble(myTank)) {
                continue;
            }
        }

        for (int j = 1; j <= 2; j++) {
            auto nextPos = myPos;
            nextPos.pos.x += j * dx;
            nextPos.pos.y += j * dy;

            if (!isValid(nextPos.pos, bot.dim)) {
                break;
            }

            if (bot.wallType[nextPos.pos.x][nextPos.pos.y] == 2) {
                break;
            }

            for (const TileVariant& object : gameState.map.tiles[nextPos.pos.x][nextPos.pos.y].objects) {
                if (std::holds_alternative<Tank>(object)) {
                    const Tank& tank = std::get<Tank>(object);
                    if (isEnemy(tank)) {
                        if (getDirId(tank.turret.direction) == (i + 2) % 4) {
                            if (!isParallel(myPos.dir, tank.turret.direction)) {
                                for (auto mov : {MoveDirection::forward, MoveDirection::backward}) {
                                    auto nxtPos = afterMove(myPos, mov);
                                    if (isValid(nxtPos.pos, bot.dim) 
                                     && !bot.isWall[nxtPos.pos.x][nxtPos.pos.y]
                                     && !willBeHitByBullet(gameState, nxtPos)
                                     && !bot.knowledgeMap.containsMine(nxtPos.pos)) 
                                    {
                                        return Move{mov};
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    return std::nullopt;
}

void Bot::initShares(const GameState& gameState) {
    assert(gameState.map.zones.size() == 1);
    Zone zone = gameState.map.zones[0];
    neutralShares = zone.status.neutral;
    for (auto [id, shares] : zone.status.teamShares) {
        if (id == teamName) {
            myShares = shares;
        }
        else {
            oppShares = shares;
        }
    }
}

float Bot::captureProb() const {
    static constexpr float MAX_PROB = 0.9;
    static constexpr float MIN_PROB = 0.1;

    float allShares = myShares + oppShares + neutralShares + 0.01;
    float result = (allShares - myShares) / allShares;

    result = std::min(result, MAX_PROB);
    result = std::max(result, MIN_PROB);

    return result;
}