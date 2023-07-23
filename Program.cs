using System;
using Raylib_cs;
using System.Numerics;
using System.Threading.Tasks;

static class Program
{
    const int GridSize = 30;
    const int CellSize = 12;

    const int TickTime = 250;

    static Cell[,] Grid;
    static Cell[,] NextGrid;

    static Vector2 GridStartingPos = new(60, 80);

    static Vector2 ButtonPosition = new(200f, 500f);
    static Vector2 ButtonSize = new(100f, 40f);

    static Vector2 ClearButtonPosition = new(200f, 550f);
    static Vector2 ClearButtonSize = new(100f, 40f);
    
    static Color ButtonPlayColor = Color.RED;
    static Color ButtonEditColor = Color.GREEN;

    static bool IsEditing = true;
    static bool IsPressing = false;

    static int Generation = 0;

    public static void Main()
    {
        Raylib.InitWindow(500, 600, "Conway's Game of Life");

        Raylib.SetTargetFPS(144);
        FillGrid(GridSize, GridStartingPos, CellSize);

        System.Timers.Timer timer = new System.Timers.Timer(TickTime);
        timer.Elapsed += Tick;
        timer.Start();

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DARKGRAY);

            Raylib.DrawText("Conway's Game of Life", 140, 12, 20, Color.WHITE);

            DrawGrid();
            DrawPlayButton();
            DrawClearButton();

            CheckButtonClick();

            // Raylib.DrawFPS(410, 10);
            Raylib.DrawText("Generation: " + Generation, 140, 40, 20, Color.GREEN);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
        
    }

    static void DrawGrid(){
        Color deadCell = Color.BLACK;
        Color aliveCell = Color.WHITE;
        foreach (Cell cell in Grid)
        {
            Raylib.DrawRectangle((int)cell.Position.X, (int)cell.Position.Y, cell.Size, cell.Size, cell.State ? aliveCell : deadCell);
        }
    }

    static void DrawPlayButton(){
        if (IsEditing){
            Raylib.DrawRectangle((int)ButtonPosition.X, (int)ButtonPosition.Y, (int)ButtonSize.X, (int)ButtonSize.Y, ButtonEditColor);
            Raylib.DrawText("Play", (int)ButtonPosition.X + 30, (int)ButtonPosition.Y + 10, 20, Color.BLACK);

        }
        else{
            Raylib.DrawRectangle((int)ButtonPosition.X, (int)ButtonPosition.Y, (int)ButtonSize.X, (int)ButtonSize.Y, ButtonPlayColor);
            Raylib.DrawText("Edit", (int)ButtonPosition.X + 30, (int)ButtonPosition.Y + 10, 20, Color.BLACK);
        }
    }

    static void DrawClearButton(){
        if (IsEditing)
        {
            Raylib.DrawRectangle((int)ClearButtonPosition.X, (int)ClearButtonPosition.Y, (int)ClearButtonSize.X, (int)ClearButtonSize.Y, Color.WHITE);
            Raylib.DrawText("Clear", (int)ClearButtonPosition.X + 30, (int)ClearButtonPosition.Y + 10, 20, Color.BLACK);

        }
    }


    static void CheckButtonClick(){
        if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && !IsPressing){
            IsPressing = true;
            // Console.WriteLine("Click");
        }
        else if(Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && IsPressing){
            IsPressing = false;
            // Console.WriteLine("Un-click");
            if( ( (Raylib.GetMousePosition().X > ButtonPosition.X) && (Raylib.GetMousePosition().X < ButtonPosition.X + ButtonSize.X) ) && ((Raylib.GetMousePosition().Y > ButtonPosition.Y) && (Raylib.GetMousePosition().Y < ButtonPosition.Y + ButtonSize.Y)) ){
                Console.WriteLine("Play Clicked");
                IsEditing = !IsEditing;
            }
            else if(((Raylib.GetMousePosition().X > ClearButtonPosition.X) && (Raylib.GetMousePosition().X < ClearButtonPosition.X + ClearButtonSize.X)) && ((Raylib.GetMousePosition().Y > ClearButtonPosition.Y) && (Raylib.GetMousePosition().Y < ClearButtonPosition.Y + ClearButtonSize.Y))){
                if(IsEditing){
                    Console.WriteLine("Clear Clicked");
                    FillGrid(GridSize, GridStartingPos, CellSize);
                }
            }
            else if (IsEditing){
                CheckGridClick();
            }
        }
    }

    static void CheckGridClick(){
        foreach (Cell cell in Grid)
        {
            if (((Raylib.GetMousePosition().X > cell.Position.X) && (Raylib.GetMousePosition().X < cell.BoundPosition.X)) && ((Raylib.GetMousePosition().Y > cell.Position.Y) && (Raylib.GetMousePosition().Y < cell.BoundPosition.Y)))
            {
                Console.WriteLine("Cell was clicked");
                cell.State = !cell.State;
            }
        }
    }

    static void FillGrid(int gridSize, Vector2 gridStartingPos, int cellSize){
        Grid = new Cell[gridSize, gridSize];
        NextGrid = new Cell[gridSize, gridSize];

        float startingX = gridStartingPos.X;
        float startingY = gridStartingPos.Y;

        float currentX = startingX;
        float currentY = startingY;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Grid[j, i] = new Cell(new Vector2(currentX, currentY), cellSize);
                currentX += cellSize + 1;
            }
            currentX = gridStartingPos.X;
            currentY += cellSize + 1;
        }

        currentX = startingX;
        currentY = startingY;
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                NextGrid[j, i] = new Cell(new Vector2(currentX, currentY), cellSize);
                currentX += cellSize + 1;
            }
            currentX = gridStartingPos.X;
            currentY += cellSize + 1;
        }
    }

    static void Tick(Object source, System.Timers.ElapsedEventArgs e){
        if(!IsEditing){
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    int aliveCount = Checker(x, y);

                    if(Grid[x, y].State && aliveCount < 2){
                        NextGrid[x, y].State = false;
                    }
                    else if( (Grid[x, y].State) && (aliveCount == 2 || aliveCount == 3) ){
                        NextGrid[x, y].State = true;
                    }
                    else if( Grid[x, y].State && aliveCount > 3){
                        NextGrid[x, y].State = false;
                    }
                    else if( !Grid[x, y].State && aliveCount == 3 ){
                        NextGrid[x, y].State = true;
                    }
                }
            }
            CopyNextToGrid();
            Generation++;
        }
    }

    static int Checker(int x, int y){
        int aliveCount = 0;
        if (x == 0)
        {
            if (y == 0)
            {
                if (Grid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y + 1].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
            else if (y == GridSize - 1)
            {
                if (Grid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
            else
            {
                if (Grid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y - 1].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
        }
        else if (x == GridSize - 1){
            if (y == 0)
            {
                if (Grid[x - 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y + 1].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
            else if (y == GridSize - 1)
            {
                if (Grid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
            else
            {
                if (Grid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
        }
        else if (x > 0 && x < GridSize){
            if( y == 0){
                if (Grid[x - 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y].State)
                {
                    aliveCount++;
                }
            }
            else if(y == GridSize - 1){
                if (Grid[x - 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y].State)
                {
                    aliveCount++;
                }
            }
            else
            {
                if (Grid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (Grid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (Grid[x + 1, y + 1].State)
                {
                    aliveCount++;
                }
            }
        }
        return aliveCount;
    }

    static void CopyGridToNext(){
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                NextGrid[x, y].CopyCell(Grid[x, y]);
            }
        }
    }

    static void CopyNextToGrid()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Grid[x, y].CopyCell(NextGrid[x, y]);
            }
        }
    }
}

class Cell
{
    public Vector2 Position { get; set; }
    public Vector2 BoundPosition { get; set; }
    public int Size { get; set; }

    public bool State { get; set; }

    public Cell(Vector2 position, int size){
        this.Position = position;
        this.Size = size;
        this.BoundPosition = new Vector2(position.X + size, position.Y + size);

        this.State = false;
    }

    public void CopyCell(Cell oldCell){
        this.Position = oldCell.Position;
        this.BoundPosition = oldCell.BoundPosition;
        this.Size = oldCell.Size;
        this.State = oldCell.State;
    }
}
