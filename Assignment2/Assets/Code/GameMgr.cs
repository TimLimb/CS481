// TODO: your name <thand556@gmail.com>
using UnityEngine;

public class GameMgr : MonoBehaviour {

    public static GameMgr instance;
    private void Awake() {
        instance = this;
    }
    
    public Pacman pacman;
    public int score;
    public int lives = 3;

    [Header("Pellets")]
    public int pelletPoints = 10;
    public int powerPelletPoints = 50;
    public int maxPellets = 0;

    [Header("Levels")]
    public TextAsset level2Maze; // level 1's tiles are already pre-baked in the scene
    public int currentLevel = 1;

    [Header("Ghosts")]
    public GhostNew[] ghosts;
    public int ghostMultiplier = 1;

    public float newGameSFXDuration = 4f;

    public void SetMaxPellets() {
        this.maxPellets = GridMgr.instance.pellets.Count;
    }


    void Start() {
        DeactivateGhosts();
        // maxPellets defaults to the scene-serialized value (0) until
        // NewRound() first calls SetMaxPellets(), 4s later. Pacman is already
        // active/moving from frame 0 though, so without this, eating even one
        // pellet before then drives maxPellets negative and PelletEaten()
        // reads that as "all pellets eaten", instantly deactivating Pacman.
        SetMaxPellets();
        NewGame();
    }
    private void Update() {
        if(lives <= 0 && Input.anyKeyDown) {

            NewGame();
        }

        if(Input.GetKeyDown(KeyCode.F2)) {
            DebugJumpToLevel2();
        }
    }

    // Assignment-required shortcut to demo level 2 directly without clearing level 1 first.
    public void DebugJumpToLevel2() {
        Debug.Log("Debug: jumping to level 2");
        currentLevel = 2;
        GridMgr.instance.LoadMaze(level2Maze);
        SetMaxPellets();
        ResetGhostsPacman(true);
    }


    public void NewGame() {
        GuiMgr.instance.NewGame();
        SfxMgr.instance.Play(SfxType.Ready);

        SetScore(0);
        SetLives(3);
        Invoke(nameof(NewRound), newGameSFXDuration);

    }

    public void NewRound() {
        GridMgr.instance.ActivatePellets();//must do this first before setting maxPellets
        SetMaxPellets();
        ResetGhostsPacman(true);

    }

    public void AdvanceToLevel2() {
        GridMgr.instance.LoadMaze(level2Maze); //rebuilds tiles+pellets and activates them
        SetMaxPellets();
        ResetGhostsPacman(true);
    }
    
    void DeactivateGhosts() {
        foreach(GhostNew ghost in ghosts) {
            ghost.gameObject.SetActive(false);
        }
    }

    public void ResetGhostsPacman() {
        ResetGhostsPacman(true);
    }

    public void ResetGhostsPacman(bool state = true) {
        ResetGhostMultiplier();
        foreach(GhostNew ghost in ghosts)
            ghost.ResetState();
        pacman.ResetState();
    }

    public void GameOver() {
        ResetGhostMultiplier();
        foreach(GhostNew ghost in ghosts)
            ghost.gameObject.SetActive(false);
        pacman.gameObject.SetActive(false);
        GuiMgr.instance.GameOver();
        SfxMgr.instance.Play(SfxType.GameOver);
    }

    public void SetScore(int score) {
        this.score = score;
        GuiMgr.instance.SetScore(score);

    }

    public void SetLives(int lives) {
        this.lives = lives;
        GuiMgr.instance.SetLives(lives);
    }

    public void GhostEaten(GhostNew ghost) {
        SetScore(score + (ghost.points * ghostMultiplier));
        ghostMultiplier++;
        ghost.isEaten = true;
        SfxMgr.instance.Play(SfxType.EatGhost);
    }

    public void PacmanEaten() {
        pacman.gameObject.SetActive(false);
        Debug.Log("Pacman Eaten");
        SetLives(lives - 1);
        SfxMgr.instance.Play(SfxType.EatPacman);
        if(lives <= 0) {
            GameOver();
        } else {
            GuiMgr.instance.ShowReady(newGameSFXDuration);
            Invoke(nameof(ResetGhostsPacman), newGameSFXDuration);
        }
    }

    public void PelletEaten(Pellet pellet) {
        //pellet.gameObject.SetActive(false);
        SfxMgr.instance.Play(SfxType.EatPellet);
        SetScore(score + pellet.points);

        maxPellets--;
        if(maxPellets <= 0) {
            Debug.Log("All pellets eaten!!!!!");
            SfxMgr.instance.Play(SfxType.Ready);
            pacman.gameObject.SetActive(false);
            GuiMgr.instance.ShowReady(newGameSFXDuration);
            if(currentLevel == 1) {
                currentLevel = 2;
                Invoke(nameof(AdvanceToLevel2), newGameSFXDuration);
            } else {
                Invoke(nameof(NewRound), newGameSFXDuration);
            }
        }
    }

    public void PowerPelletEaten(PowerPellet ppellet) {
        SfxMgr.instance.Play(SfxType.EatPowerPellet);
        PelletEaten(ppellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), ppellet.duration);
        FrightenGhosts();

        SetScore(score + powerPelletPoints);
        //ToDo
    }

    public void FrightenGhosts() {
        SfxMgr.instance.Play(SfxType.GhostScared);
        foreach(GhostNew ghost in ghosts) {
            ghost.State = GhostState.Frightened;
        }
    }


    void ResetGhostMultiplier() {
        ghostMultiplier = 1;
    }

}
