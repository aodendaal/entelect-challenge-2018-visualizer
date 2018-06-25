using Newtonsoft.Json;
using StarterBot.Entities;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Panels & Text")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public Text roundText;
    public GameObject gameOverPanel;

    [Header("Prefabs")]
    public GameObject attackPrefab;
    public GameObject teslaPrefab;
    public GameObject defendPrefab;
    public GameObject energyPrefab;
    public GameObject missilePrefab;

    [Header("Camera")]
    public GameObject freeLookCamera;

    [Header("Particles")]
    public GameObject explosionPrefab;

    [Header("Player A")]
    public GameObject playerAPanel;
    public TextMesh playerANameMesh;
    public Text playerAName;
    public Text playerAHealth;
    public Text playerAEnergy;
    public Text playerAScore;

    [Header("Player B")]
    public GameObject playerBPanel;
    public TextMesh playerBNameMesh;
    public Text playerBName;
    public Text playerBHealth;
    public Text playerBEnergy;
    public Text playerBScore;


    [Header("Step Controls")]
    public GameObject speedPanel;
    public Button stepButton;
    public Slider speedSlider;

    private string directory;
    private int round = 0;

    private bool hasGameStarted = false;
    private float roundRate = 0.5f;
    private float roundTime = 0;

    private const string pathKey = "LastPath";

    private void Start()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        playerAPanel.SetActive(false);
        playerBPanel.SetActive(false);
        speedPanel.SetActive(false);

        freeLookCamera.SetActive(false);

        if (PlayerPrefs.HasKey(pathKey))
        {
            GameObject.Find("InputField").GetComponent<InputField>().text = PlayerPrefs.GetString(pathKey);
        }

        speedSlider.value = roundRate;
        stepButton.interactable = false;
    }

    public void Start_Click()
    {
        directory = GameObject.Find("InputField").GetComponent<InputField>().text;
        PlayerPrefs.SetString(pathKey, directory);

        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        playerAPanel.SetActive(true);
        playerBPanel.SetActive(true);
        speedPanel.SetActive(true);

        freeLookCamera.SetActive(true);

        hasGameStarted = true;
    }

    public void Restart_Click()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit_Click()
    {
        Application.Quit();
    }

    public void Speed_OnChange()
    {
        if (speedSlider.value == 1f)
        {
            roundRate = 0.001f;
        }
        else if (speedSlider.value == 0f)
        {
            roundRate = 0;
        }
        else
        {
            roundRate = 1f - speedSlider.value;
        }

        if (roundRate == 0)
        {
            stepButton.interactable = true;
        }
        else
        {
            stepButton.interactable = false;
        }
    }

    public void Step_Click()
    {
        ProcessNextFile();
    }

    private void Update()
    {
        if (hasGameStarted && roundRate > 0f)
        {
            if (Time.time > roundTime)
            {
                ProcessNextFile();

                roundTime = Time.time + roundRate;
            }
        }
    }

    private void ProcessNextFile()
    {
        var roundDirectory = $"Round {round.ToString().PadLeft(3, '0')}";

        roundDirectory = Path.Combine(directory, roundDirectory);

        if (Directory.Exists(roundDirectory))
        {
            var playerDirectories = Directory.GetDirectories(roundDirectory);

            if (round == 0)
            {
                SetPlayerNames(playerDirectories);
            }

            var stateFile = Path.Combine(playerDirectories[0], "JsonMap.json");

            var gameState = JsonConvert.DeserializeObject<GameState>(File.ReadAllText(stateFile));

            ProcessRound(gameState);

            round++;
            roundText.text = round.ToString();
        }
        else
        {
            hasGameStarted = false;
            gameOverPanel.SetActive(true);
        }
    }

    private void SetPlayerNames(string[] playerDirectories)
    {
        var index = playerDirectories[0].LastIndexOf(Path.DirectorySeparatorChar);
        var name = playerDirectories[0].Substring(index, playerDirectories[0].Length - index);
        name = name.Split('-')[1].Trim();

        playerANameMesh.text = name;

        index = playerDirectories[1].LastIndexOf(Path.DirectorySeparatorChar);
        name = playerDirectories[1].Substring(index, playerDirectories[1].Length - index);
        name = name.Split('-')[1].Trim();

        playerBNameMesh.text = name;
    }

    private void ProcessRound(GameState gameState)
    {
        UpdateDisplay(gameState.Players[0], gameState.Players[1]);

        for (var x = 0; x < gameState.GameDetails.MapWidth; x++)
        {
            for (var y = 0; y < gameState.GameDetails.MapHeight; y++)
            {
                var cell = gameState.GameMap[y][x];
                var pos = new Vector3(cell.X * 2f, 0f, cell.Y * 2f);

                if (cell.Buildings.Count > 0)
                {
                    CreateBuilding(pos, cell);
                }
                else
                {
                    DestroyBuilding(pos, cell);
                }

                if (cell.Missiles.Count > 0)
                {
                    CreateMissiles(pos, cell);
                }
                else
                {
                    DestroyMissiles(pos, cell);
                }
            }
        }
    }

    private void UpdateDisplay(Player player1, Player player2)
    {
        if (player1.Health.ToString() != playerAHealth.text)
        {
            playerAHealth.color = Color.red;
        }
        else
        {
            playerAHealth.color = Color.yellow;
        }
        playerAHealth.text = player1.Health.ToString();
        playerAEnergy.text = player1.Energy.ToString();
        playerAScore.text = player1.Score.ToString();


        if (player2.Health.ToString() != playerBHealth.text)
        {
            playerBHealth.color = Color.red;
        }
        else
        {
            playerBHealth.color = Color.yellow;
        }
        playerBHealth.text = player2.Health.ToString();
        playerBEnergy.text = player2.Energy.ToString();
        playerBScore.text = player2.Score.ToString();
    }

    private void DestroyMissiles(Vector3 pos, CellStateContainer cell)
    {
        pos.y += 1f;

        if (Physics.CheckSphere(pos, 1f, 1 << 10))
        {
            var colliders = Physics.OverlapSphere(pos, 1f, 1 << 10);
            if (colliders.Length > 0)
            {
                Destroy(colliders[0].gameObject);
            }
        }
    }

    private void CreateMissiles(Vector3 pos, CellStateContainer cell)
    {
        pos.y += 1f;

        if (!Physics.CheckSphere(pos, 1f, 1 << 10))
        {
            var go = Instantiate(missilePrefab, pos, Quaternion.identity);

            if (cell.Missiles[0].PlayerType == StarterBot.Enums.PlayerType.A)
            {
                go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                go.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }

            go.transform.SetParent(transform);
        }
    }

    private void DestroyBuilding(Vector3 pos, CellStateContainer cell)
    {
        if (Physics.CheckSphere(pos, 1f, 1 << 9))
        {
            var colliders = Physics.OverlapSphere(pos, 1f, 1 << 9);

            if (colliders.Length > 0)
            {
                Destroy(colliders[0].gameObject);
                var go = Instantiate(explosionPrefab, pos, Quaternion.identity);
                go.transform.SetParent(transform);
                Destroy(go, 2f);
            }
        }
    }

    private void CreateBuilding(Vector3 pos, CellStateContainer cell)
    {
        var playSound = true;

        if (Physics.CheckSphere(pos, 1f, 1 << 9))
        {
            var colliders = Physics.OverlapSphere(pos, 1f, 1 << 9);

            if (colliders.Length > 0)
            {
                var previous = colliders[0].gameObject;
                if (previous.transform.localScale != Vector3.one)
                {
                    playSound = false;
                    Destroy(previous);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        var type = cell.Buildings[0].BuildingType;

        GameObject prefab = null;

        if (type == StarterBot.Enums.BuildingType.Attack)
        {
            prefab = attackPrefab;
        }
        else if (type == StarterBot.Enums.BuildingType.Defense)
        {
            prefab = defendPrefab;

        }
        else if (type == StarterBot.Enums.BuildingType.Energy)
        {
            prefab = energyPrefab;
        }
        else if (type == StarterBot.Enums.BuildingType.Tesla)
        {
            prefab = teslaPrefab;
        }

        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.transform.SetParent(transform);

        if (cell.Buildings[0].ConstructionTimeLeft > -1)
        {
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        else
        {
            go.GetComponent<AudioSource>().Stop();
        }
    }
}