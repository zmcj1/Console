﻿#include "MinConsoleNative.hpp"
using namespace std;

constexpr int WIDTH = 60;
constexpr int HEIGHT = 30;

enum class MonsterType
{
    Zombie = 1,
    Ghost = 2,
};

class MonsterBase
{
public:
    Vector2 position;
    MonsterType  type;

    float HP;
    float Speed;
    float Attack;
    float Armor;
};

class Zombie : public MonsterBase
{
public:

};

class Ghost : public MonsterBase
{
public:


};

enum class GameState
{
    None = 0,
    Menu = 1,
    Game = 2,
    Result = 3,
};

class Corn : public ConsoleEngine
{
private:
    GameState state = GameState::Menu;
    CharacterController* playerController = nullptr;
    CellRenderer* renderer = nullptr;
    vector<Vector2> maze;
    vector<Vector2> ground;

    vector<MonsterBase> monsters;

public:
    void OnStart() override
    {
        //initialization
        renderer = new CellRenderer(WIDTH, HEIGHT, CellRendererMode::TrueColor);
        //start game by default
        maze = MazeGenerator::GenerateMaze(WIDTH - 1, HEIGHT - 1);
        state = GameState::Game;

        ground = MazeGenerator::GenerateGround(maze, WIDTH - 1, HEIGHT - 1);

        //generate player
        playerController = new CharacterController({ 1, 1 }, 10);
        //playerController = new CharacterController({ WIDTH - 4, HEIGHT - 4 }, 10);

        //test:generate 10 monsters once
        for (size_t i = 0; i < 10; i++)
        {
            GenerateMonster(Vector2(), MonsterType::Zombie);
        }
    }

    void GenerateMonster(Vector2 position, MonsterType type)
    {

    }

    void OnUpdate(float deltaTime) override
    {
        if (state == GameState::Game)
        {
            //store beforePos here, later we shall deal with collision with maze.
            Vector2 beforePos = playerController->position;

            playerController->Move4(deltaTime);

            //border check
            if (playerController->position.x < 0 || playerController->position.x > WIDTH - 1 ||
                playerController->position.y < 0 || playerController->position.y > HEIGHT - 1)
            {
                playerController->position = beforePos;
            }

            //collision check
            for (size_t i = 0; i < maze.size(); i++)
            {
                if (playerController->position == maze[i])
                {
                    playerController->position = beforePos;
                    break;
                }
            }

            //win check
            if (playerController->position == Vector2(WIDTH - 3, HEIGHT - 3))
            {
                //you win!
                state = GameState::Result;
                //redraw scene
                DrawScene();
                //show you win
                renderer->DrawString(Vector2(WIDTH / 2, HEIGHT / 2), L"you win!", { 255,0,0 }, { 18,133,166 }, false);
                renderer->Render();
            }
        }

        //draw scene
        if (state == GameState::Game)
        {
            DrawScene();
        }
    }

    void DrawScene()
    {
        if (Input::GetKey('X'))
        {
            //清空屏幕
            console.Clear();
            //重新绘制游戏
            renderer->Clear();
            renderer->Render();
        }

        renderer->Clear();

        for (size_t i = 0; i < maze.size(); i++)
        {
            renderer->Draw(maze[i], Cell(L'X', { 0,0,0 }, { 0,255,33 }));
        }

        for (size_t i = 0; i < ground.size(); i++)
        {
            renderer->Draw(ground[i], Cell(L'L', { 0,0,0 }, { 255,0,33 }));
        }

        renderer->Draw(playerController->position, Cell(L'K', { 255,33,33 }, { 0,0,0 }));

        renderer->Render();
    }

    void OnDestroy() override
    {
        delete playerController;
        delete renderer;
    }
};

int main()
{
    Corn corn;

    console.SetConsoleCursorVisible(false);

    corn.Construct(L"Mr. Kukin(Corn)", WIDTH, HEIGHT);
    corn.StartLoop();

    //bool win11 = winVersion.IsWindows11();
    //console.WriteLine(to_wstring(win11));
    //console.ReadLine();

    //ConRegistry::DeleteConsoleRegistry();

    //int type = (int)console.GetConsoleType();
    //console.WriteLine(to_wstring(type));
    //console.ReadLine();

    return 0;
}