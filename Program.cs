using System;
using Raylib_cs;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

static class Program
{
    const int GridSize = 30;
    const int CellSize = 12;

    const int TickTime = 150;

    static Cell[,] GameGrid;
    static Cell[,] NextGenGrid;

    static Vector2 GridStartingPos = new(60, 80);

    static Vector2 ButtonSize = new(100f, 40f);

    static Vector2 PlayButtonPosition = new(200f, 500f);
    static Vector2 ClearButtonPosition = new(PlayButtonPosition.X, PlayButtonPosition.Y + ButtonSize.Y + 5);
    static Vector2 SaveButtonPosition = new(PlayButtonPosition.X - ButtonSize.X - 5, PlayButtonPosition.Y);
    static Vector2 LoadPosition = new(SaveButtonPosition.X, SaveButtonPosition.Y + ButtonSize.Y + 5);
    static Vector2 NextGenButtonPosition = new(PlayButtonPosition.X + ButtonSize.X + 5, PlayButtonPosition.Y);

    static Color ButtonPlayColor = Color.RED;
    static Color ButtonEditColor = Color.GREEN;

    static Color DeadCellColor = Color.BLACK;
    static Color AliveCellColor = Color.WHITE;

    static bool IsEditing = true;
    static bool IsPressing = false;

    static int Generation = 0;

    public static void Main()
    {
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(500, 600, "Conway's Game of Life");

        Raylib.SetTargetFPS(144);
        FillGrid(GridSize, GridStartingPos, CellSize);

        System.Timers.Timer timer = new System.Timers.Timer(TickTime);
        timer.Elapsed += Tick;
        timer.Start();
        
        Console.Clear();

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.DARKGRAY);

            Raylib.DrawText("Conway's Game of Life", 140, 12, 20, Color.WHITE);

            DrawGrid();
            DrawPlayButton();
            DrawClearButton();
            DrawSaveLoadButton();
            DrawNextGenButton();

            CheckButtonClick();

            // Raylib.DrawFPS(410, 10);
            Raylib.DrawText("Generation: " + Generation, 140, 40, 20, Color.GREEN);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
        
    }

    static void DrawGrid(){
        foreach (Cell cell in GameGrid)
        {
            Raylib.DrawRectangle((int)cell.Position.X, (int)cell.Position.Y, cell.Size, cell.Size, cell.State ? AliveCellColor : DeadCellColor);
        }
    }

    static void DrawPlayButton(){
        if (IsEditing){
            Raylib.DrawRectangle((int)PlayButtonPosition.X, (int)PlayButtonPosition.Y, (int)ButtonSize.X, (int)ButtonSize.Y, ButtonEditColor);
            Raylib.DrawText("Play", (int)PlayButtonPosition.X + 30, (int)PlayButtonPosition.Y + 10, 20, Color.BLACK);

        }
        else{
            Raylib.DrawRectangle((int)PlayButtonPosition.X, (int)PlayButtonPosition.Y, (int)ButtonSize.X, (int)ButtonSize.Y, ButtonPlayColor);
            Raylib.DrawText("Edit", (int)PlayButtonPosition.X + 30, (int)PlayButtonPosition.Y + 10, 20, Color.BLACK);
        }
    }

    static void DrawClearButton(){
        if (IsEditing)
        {
            Raylib.DrawRectangle((int)ClearButtonPosition.X, (int)ClearButtonPosition.Y, (int)ButtonSize.X, (int)ButtonSize.Y, Color.WHITE);
            Raylib.DrawText("Clear", (int)ClearButtonPosition.X + 30, (int)ClearButtonPosition.Y + 10, 20, Color.BLACK);
        }
    }

    static void DrawSaveLoadButton(){
        if (IsEditing)
        {
            Raylib.DrawRectangle((int)(SaveButtonPosition.X), (int)(SaveButtonPosition.Y), (int)ButtonSize.X, (int)ButtonSize.Y, Color.GREEN);
            Raylib.DrawText("Save", (int)(SaveButtonPosition.X + 30), (int)SaveButtonPosition.Y + 10, 20, Color.BLACK);

            Raylib.DrawRectangle((int)(LoadPosition.X), (int)(LoadPosition.Y), (int)ButtonSize.X, (int)ButtonSize.Y, Color.BLUE);
            Raylib.DrawText("Load", (int)(LoadPosition.X + 30), (int)LoadPosition.Y + 10, 20, Color.BLACK);
        }
    }

    static void DrawNextGenButton(){
        if(IsEditing){
            Raylib.DrawRectangle((int)(NextGenButtonPosition.X), (int)(NextGenButtonPosition.Y), (int)ButtonSize.X, (int)ButtonSize.Y, Color.YELLOW);
            Raylib.DrawText("Next Gen", (int)(NextGenButtonPosition.X + 5), (int)NextGenButtonPosition.Y + 10, 20, Color.BLACK);
        }
    }

    static void CheckButtonClick(){
        if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && !IsPressing){
            IsPressing = true;
            // Console.WriteLine("Click");
        }
        else if(Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT) && IsPressing){
            IsPressing = false;
            if( ( (Raylib.GetMousePosition().X > PlayButtonPosition.X) && (Raylib.GetMousePosition().X < PlayButtonPosition.X + ButtonSize.X) ) && ((Raylib.GetMousePosition().Y > PlayButtonPosition.Y) && (Raylib.GetMousePosition().Y < PlayButtonPosition.Y + ButtonSize.Y)) ){
                // Console.WriteLine("Play Clicked");
                IsEditing = !IsEditing;
            }
            else if(((Raylib.GetMousePosition().X > ClearButtonPosition.X) && (Raylib.GetMousePosition().X < ClearButtonPosition.X + ButtonSize.X)) && ((Raylib.GetMousePosition().Y > ClearButtonPosition.Y) && (Raylib.GetMousePosition().Y < ClearButtonPosition.Y + ButtonSize.Y))){
                if(IsEditing){
                    // Console.WriteLine("Clear Clicked");
                    FillGrid(GridSize, GridStartingPos, CellSize);
                }
            }
            else if (((Raylib.GetMousePosition().X > SaveButtonPosition.X) && (Raylib.GetMousePosition().X < SaveButtonPosition.X + ButtonSize.X)) && ((Raylib.GetMousePosition().Y > SaveButtonPosition.Y) && (Raylib.GetMousePosition().Y < SaveButtonPosition.Y + ButtonSize.Y)))
            {
                if (IsEditing)
                {
                    GridSavedData data = new GridSavedData(GameGrid);
                    SaveData(data);
                }
            }
            else if (((Raylib.GetMousePosition().X > LoadPosition.X) && (Raylib.GetMousePosition().X < LoadPosition.X + ButtonSize.X)) && ((Raylib.GetMousePosition().Y > LoadPosition.Y) && (Raylib.GetMousePosition().Y < LoadPosition.Y + ButtonSize.Y)))
            {
                if (IsEditing)
                {
                    LoadData();
                }
            }
            else if (((Raylib.GetMousePosition().X > NextGenButtonPosition.X) && (Raylib.GetMousePosition().X < NextGenButtonPosition.X + ButtonSize.X)) && ((Raylib.GetMousePosition().Y > NextGenButtonPosition.Y) && (Raylib.GetMousePosition().Y < NextGenButtonPosition.Y + ButtonSize.Y)))
            {
                // Console.WriteLine("Click");
                Tick();
            }
            else if (IsEditing){
                CheckGridClick();
            }
        }
    }

    static void CheckGridClick(){
        foreach (Cell cell in GameGrid)
        {
            if (((Raylib.GetMousePosition().X > cell.Position.X) && (Raylib.GetMousePosition().X < cell.BoundPosition.X)) && ((Raylib.GetMousePosition().Y > cell.Position.Y) && (Raylib.GetMousePosition().Y < cell.BoundPosition.Y)))
            {
                // Console.WriteLine("Cell was clicked");
                cell.State = !cell.State;
            }
        }
    }

    static void FillGrid(int gridSize, Vector2 gridStartingPos, int cellSize){
        GameGrid = new Cell[gridSize, gridSize];
        NextGenGrid = new Cell[gridSize, gridSize];

        float startingX = gridStartingPos.X;
        float startingY = gridStartingPos.Y;

        float currentX = startingX;
        float currentY = startingY;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameGrid[j, i] = new Cell(new Vector2(currentX, currentY), cellSize);
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
                NextGenGrid[j, i] = new Cell(new Vector2(currentX, currentY), cellSize);
                currentX += cellSize + 1;
            }
            currentX = gridStartingPos.X;
            currentY += cellSize + 1;
        }

        Generation = 0;
    }

    static void Tick(Object source, System.Timers.ElapsedEventArgs e){
        if(!IsEditing){
            CopyGridToNext();
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    int aliveCount = Checker(x, y);

                    if(GameGrid[x, y].State && aliveCount < 2){
                        NextGenGrid[x, y].State = false;
                    }
                    else if( (GameGrid[x, y].State) && (aliveCount == 2 || aliveCount == 3) ){
                        NextGenGrid[x, y].State = true;
                    }
                    else if( GameGrid[x, y].State && aliveCount > 3){
                        NextGenGrid[x, y].State = false;
                    }
                    else if( !GameGrid[x, y].State && aliveCount == 3 ){
                        NextGenGrid[x, y].State = true;
                    }
                }
            }
            CopyNextToGrid();
            Generation++;
        }
    }
   
    static void Tick()
    {
        if (IsEditing)
        {
            CopyGridToNext();
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    int aliveCount = Checker(x, y);

                    if (GameGrid[x, y].State && aliveCount < 2)
                    {
                        NextGenGrid[x, y].State = false;
                    }
                    else if ((GameGrid[x, y].State) && (aliveCount == 2 || aliveCount == 3))
                    {
                        NextGenGrid[x, y].State = true;
                    }
                    else if (GameGrid[x, y].State && aliveCount > 3)
                    {
                        NextGenGrid[x, y].State = false;
                    }
                    else if (!GameGrid[x, y].State && aliveCount == 3)
                    {
                        NextGenGrid[x, y].State = true;
                    }
                }
            }
            CopyNextToGrid();
            Generation++;
        }
    }

    // Edit Checker to make it loop around isntead of stopping at the borders
    static int Checker(int x, int y){
        int aliveCount = 0;
        if (x == 0)
        {
            if (y == 0)
            {
                if (GameGrid[GridSize - 1, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[GridSize - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[GridSize - 1, y].State)
                {
                    aliveCount++;
                }
            }
            else if (y == GridSize - 1)
            {
                if (GameGrid[GridSize - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1 , y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[GridSize - 1, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[ GridSize - 1, y].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
            else
            {
                if (GameGrid[GridSize - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[GridSize - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[GridSize - 1, y].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
        }
        else if (x == GridSize - 1){
            if (y == 0)
            {
                if (GameGrid[x - 1, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
            else if (y == GridSize - 1)
            {
                if (GameGrid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
            else
            {
                if (GameGrid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[0, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y].State)
                {
                    aliveCount++;
                }
                // Console.WriteLine($"{x}, {y} aliveCount {aliveCount}");
            }
        }
        else if (x > 0 && x < GridSize){
            if( y == 0){
                if (GameGrid[x - 1, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, GridSize - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y].State)
                {
                    aliveCount++;
                }
            }
            else if(y == GridSize - 1){
                if (GameGrid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, 0].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y].State)
                {
                    aliveCount++;
                }
            }
            else
            {
                if (GameGrid[x - 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y - 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x - 1, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x, y + 1].State)
                {
                    aliveCount++;
                }
                if (GameGrid[x + 1, y + 1].State)
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
                NextGenGrid[x, y].CopyCell(GameGrid[x, y]);
            }
        }
    }

    static void CopyNextToGrid()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                GameGrid[x, y].CopyCell(NextGenGrid[x, y]);
            }
        }
    }

    public static void SaveData(GridSavedData Data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = "conway.life";

        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, Data);
        stream.Close();
        Console.WriteLine("Current game state saved!");
    }

    public static void LoadData()
    {
        string path = "conway.life";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GridSavedData data = formatter.Deserialize(stream) as GridSavedData;
            stream.Close();
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    GameGrid[x, y].CopyCell(data.grid[x, y]);
                    NextGenGrid[x, y].CopyCell(data.grid[x, y]);
                }
            }

            Generation = 0;
            Console.WriteLine("Loaded");
        }
        else
        {
            Console.WriteLine("Save file not found, try saving before loading");
        }
    }
}

[Serializable]
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

[Serializable]
class GridSavedData{
    public Cell[,] grid;

    public GridSavedData(Cell[,] grid){
        this.grid = grid;
    }
}

[Serializable]
class Vector2{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float X, float Y){
        this.X = X;
        this.Y = Y;
    }
}
