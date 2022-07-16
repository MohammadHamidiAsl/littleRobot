using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MHamidi;
using MHamidi.Helper;
using UnityEngine;


public interface ILevelManger
{
    public Level currentLevel { get; set; }
    void Intereact();
    public Vector3Int GetFrontOfPlayerPosition();

    public int GetFrontOfPlayerHeight();
    public int GetPlayerCurrentHeight();

    public void CreatLevel(Level level, Action<GameObject> setSubjectOfCommand);
    public void ResetLevel();
    public bool CheckIfGameEnded();
    bool IsAvailable(ICommand command);
    void Submit(ICommand command);
}


public class PlayeController
{
}

public class LevelManger3D : MonoBehaviour, ILevelManger
{
    public static event Action CurrentLevelEnded;

    public Ease ease;
    public Level currentLevel { get; set; }

    public static event Action<List<ICommand>> AddAvilableCommand;
    public static event Action<int> AddBufferSize;
    public static event Action<int> AddP1Size;
    public static event Action<int> AddP2Size;
    [SerializeField] private bool DebugingIsOn = false;
    private List<GameObject> Cells;
    private GameCell[,] gameCells;
    private List<GameCell> currentLevelInteractable;
    public GameObject Player;
    [SerializeField] private Vector3Int playerPos;
    public Vector3Int PlayeForward;
    [SerializeField] private GameObject PlayerPrefab;

    public static ILevelManger Instance;

    private void Awake()
    {
        Instance = this;
        Cells = new List<GameObject>();
        currentLevelInteractable = new List<GameCell>();
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public int GetBackOfPlayerHeight()
    {
        throw new NotImplementedException();
    }

    public void CreatLevel(Level level, Action<GameObject> Playerreference)
    {
        currentLevelInteractable.Clear();

        if (gameCells is null)
        {
            gameCells = new GameCell[level.width, level.height];
        }

        currentLevel = level;

        StartCoroutine(CreatLevelWithDelay(level, () => { Playerreference?.Invoke(Player); }));
    }

    public void ResetLevel()
    {
        Player.transform.position = new Vector3(currentLevel.startX,
            1f + ((currentLevel.LevelLayout[currentLevel.startX, currentLevel.startY].cellHeight - 1) * .2f),
            currentLevel.startY);


        foreach (var item in currentLevelInteractable)
        {
            item.TurnOff();
        }
    }

    private IEnumerator CreatLevelWithDelay(Level level, Action OnComplet)
    {
        yield return StartCoroutine(ClearLevelWithDelay());

        for (int i = 0; i < level.width; i++)
        {
            for (int j = 0; j < level.height; j++)
            {
                if (level.LevelLayout[i, j].cellHeight > 0)
                {
                    var cell = Pool.Instance.Get("Cell" + level.LevelLayout[i, j].cellHeight);
                    cell.gameObject.SetActive(true);
                    var cellType = level.LevelLayout[i, j].Type == CellType.Interactable
                        ? GameCellType.InteractableOff
                        : GameCellType.Simple;
                    SetupCell(cell, i, j, level.LevelLayout[i, j].cellHeight, cellType);


                    if (level.startX == i && level.startY == j)
                    {
                        playerPos = new Vector3Int(i,level.LevelLayout[i,j].cellHeight,j);
                        PlayeForward = level.direction switch
                        {
                            PlayerDirection.Down => new Vector3Int(0, 0, 1),
                            PlayerDirection.Up => new Vector3Int(0, 0, -1),
                            PlayerDirection.Left => new Vector3Int(1, 0, 0),
                            _ => new Vector3Int(-1, 0, 0),
                        };
                        Player = Instantiate(PlayerPrefab, new Vector3(i, -10, j), Quaternion.identity);
                        Player.transform.DOMove(new Vector3(i, 1, j), .8f).SetEase(ease);
                        Cells.Add(Player);
                    }
                }

                yield return new WaitForSeconds(.03f);
            }

            OnComplet?.Invoke();
        }

        yield return null;
    }

    private IEnumerator ClearLevelWithDelay()
    {
        foreach (var item in Cells)
        {
            item.transform.DOMove(item.transform.position - (item.transform.up * 10), .8f).SetEase(ease).OnComplete(
                () =>
                {
                    item.transform.SetParent(Pool.Instance.gameObject.transform, false);
                    item.gameObject.SetActive(false);
                });
            yield return new WaitForSeconds(.02f);
        }

        Cells.Clear();
    }

    private void SetupCell(GameObject cell, int i, int j, int height, GameCellType cellType)
    {
        Cells.Add(cell);
        gameCells[i, j] = cell.GetComponent<GameCell>();
        gameCells[i, j].Setup(cellType);
        gameCells[i, j].SetupDebugerPart(new Vector2Int(i, j), height, DebugingIsOn);
        if (cellType is GameCellType.InteractableOff)
        {
            currentLevelInteractable.Add(gameCells[i, j]);
        }

        cell.transform.position = new Vector3(i, -10, j);
        cell.transform.DOMove(new Vector3(i, 0, j), .8f).SetEase(ease);
    }

    public bool CheckIfGameEnded()
    {
        foreach (var interactableCell in currentLevelInteractable)
        {
            if (interactableCell.type == GameCellType.InteractableOff)
            {
                Util.ShowMessag($" Game Not Ended", TextColor.Red);
                return false;
            }
        }

        Util.ShowMessag($" Game Is Ended", TextColor.Green);
        CurrentLevelEnded?.Invoke();
        return true;
    }

    public void Intereact()
    {
        var pos = GetPlayerPosition();
        //gameCells[(int)Player.transform.position.x, (int)Player.transform.position.z].Interact();
        gameCells[pos.x, pos.z].Interact();
        CheckIfGameEnded();
    }


    public Vector3Int GetFrontOfPlayerPosition()
    {
        var fp = GetPlayerPosition() + GetLocalForwardOfPlayer();
        return fp;
    }

    public Vector3Int GetPlayerPosition()
    {
        return playerPos;
    }


    public void Rotate(bool isRight)
    {
        if (isRight)
        {
            switch (PlayeForward.x)
            {
                case 0 when PlayeForward.y == 1:
                    PlayeForward = new Vector3Int(1, 0,0);
                    break;
                case 0 when PlayeForward.y == -1:
                    PlayeForward = new Vector3Int(-1,0, 0);
                    break;
                case 1 when PlayeForward.y == 0:
                    PlayeForward = new Vector3Int(0, 0,-1);
                    break;
                case -1 when PlayeForward.y == 0:
                    PlayeForward = new Vector3Int(0, 0,1);
                    break;
            }
        }
        else
        {
            switch (PlayeForward.x)
            {
                case 0 when PlayeForward.y == 1:
                    PlayeForward = new Vector3Int(-1,0, 0);
                    break;
                case 0 when PlayeForward.y == -1:
                    PlayeForward = new Vector3Int(1, 0,0);
                    break;
                case 1 when PlayeForward.y == 0:
                    PlayeForward = new Vector3Int(0, 0,1);
                    break;
                case -1 when PlayeForward.y == 0:
                    PlayeForward = new Vector3Int(0, 0,-1);
                    break;
            }
        }
    }

    public Vector3Int GetLocalForwardOfPlayer()
    {
        return PlayeForward;
    }


    public int GetFrontOfPlayerHeight()
    {
        var fh = currentLevel.LevelLayout[GetFrontOfPlayerPosition().x, GetFrontOfPlayerPosition().z].cellHeight;
        return fh;
    }


    public int GetPlayerCurrentHeight()
    {
        //var position = GetPlayerPosition();
        //var ch = currentLevel.LevelLayout[position.x, position.z].cellHeight;
        return GetPlayerPosition().y;
    }
    
    
    public bool IsAvailable(ICommand command)
    {
        command.Requirement(currentLevel.height, currentLevel.width, GetPlayerPosition(), GetLocalForwardOfPlayer(),
            GetPlayerCurrentHeight(), GetFrontOfPlayerHeight());
        return false;
    }

    
        
    public void Submit(ICommand command)
    {
        command.ExecutionInstruction(Player, GetPlayerPosition(), GetLocalForwardOfPlayer(),
            GetPlayerCurrentHeight(), GetFrontOfPlayerHeight());
    }
}