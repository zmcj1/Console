﻿#pragma once

#include "MinDefines.h"
#include "Vector2.h"
#include <vector>

namespace MinConsoleNative
{
    class MazeGenerator
    {
    private:
        static void LinkTo(Vector2& point, std::vector<Vector2>& keyPoints, std::vector<Vector2>& arrivedPoints, std::vector<Vector2>& emptyPoints);

    public:
        static std::vector<Vector2> GenerateKeyPoints(int width, int height);

        //return:the positions of obstacles as a std::vector<Vector2>
        //NOTICE:The generation algorithm requires the length and width of the maze to be an odd number!(3, 5, 7, 9, 11, 13, 15...)
        static std::vector<Vector2> GenerateMaze(int width, int height);
    };
}