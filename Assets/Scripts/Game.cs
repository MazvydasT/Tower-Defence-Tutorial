using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Vector2Int boardSize = new(11, 11);

    [SerializeField]
    GameBoard board = default;

    [SerializeField]
    GameTileContentFactory tileContentFactory = default;

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
    }

    private void OnValidate()
    {
        if (boardSize.x < 2) boardSize.x = 2;
        if (boardSize.y < 2) boardSize.y = 2;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleTouch();

        else if (Input.GetMouseButtonDown(1))
            HandleAlternativeTouch();

        if (Input.GetKeyDown(KeyCode.V))
            board.ShowPaths = !board.ShowPaths;

        if (Input.GetKeyDown(KeyCode.G))
            board.ShowGrid = !board.ShowGrid;
    }

    private void HandleTouch()
    {
        var tile = board.GetTile(TouchRay);

        if (tile != null)
            board.ToggleWall(tile);
    }

    private void HandleAlternativeTouch()
    {
        var tile = board.GetTile(TouchRay);

        if (tile != null)
            board.ToggleDestination(tile);
    }
}
