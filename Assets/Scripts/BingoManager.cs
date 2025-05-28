using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class BingoManager : MonoBehaviourPunCallbacks
{
    public GameObject myPanel;           // Set in Inspector: Panel with 25 buttons for local player
    public GameObject opponentPanel;     // Set in Inspector: Panel with 25 buttons for opponent
    public Text instructionText;

    private List<Button> myButtons = new();
    private List<Button> opponentButtons = new();
    private Dictionary<int, Button> myNumberMap = new();
    private Dictionary<int, Button> opponentNumberMap = new();

    private Button[,] myGrid = new Button[5, 5];
    private Button[,] opponentGrid = new Button[5, 5];

    private int currentNumber = 1;
    private bool isMyTurn = false;
    private bool myPlacementDone = false;
    private bool opponentPlacementDone = false;
    private bool placementPhase = true;

    void Start()
    {
        ExtractButtons();
        SetupGrids();
        SetupButtonListeners();
        opponentPanel.SetActive(false);
        instructionText.text = "Place numbers (1 to 25)";
    }


    void ExtractButtons()
    {
        myButtons.AddRange(myPanel.GetComponentsInChildren<Button>());
        opponentButtons.AddRange(opponentPanel.GetComponentsInChildren<Button>());

        // Clean up opponent board display
        foreach (var btn in opponentButtons)
        {
            btn.GetComponentInChildren<Text>().text = "";
        }
    }

    void SetupGrids()
    {
        for (int i = 0; i < 25; i++)
        {
            int row = i / 5;
            int col = i % 5;
            myGrid[row, col] = myButtons[i];
            opponentGrid[row, col] = opponentButtons[i];
        }
    }

    void SetupButtonListeners()
    {
        for (int i = 0; i < myButtons.Count; i++)
        {
            int index = i;
            myButtons[i].onClick.AddListener(() => OnMyButtonClick(index));
        }
    }

    void OnMyButtonClick(int index)
    {
        Button btn = myButtons[index];

        if (placementPhase && string.IsNullOrEmpty(btn.GetComponentInChildren<Text>().text))
        {
            btn.GetComponentInChildren<Text>().text = currentNumber.ToString();
            myNumberMap[currentNumber] = btn;

            // Tell opponent to set this number on the same index (without showing it)
            photonView.RPC("SetOpponentNumber", RpcTarget.Others, index, currentNumber);

            currentNumber++;
            if (currentNumber <= 25)
            {
                instructionText.text = $"Next number: {currentNumber}";
            }
            else
            {
                myPlacementDone = true;
                instructionText.text = "Waiting for opponent...";
                photonView.RPC("NotifyPlacementDone", RpcTarget.Others);
                CheckStartGame();
            }
        }
        else if (!placementPhase && isMyTurn && btn.interactable)
        {
            int number = int.Parse(btn.GetComponentInChildren<Text>().text);
            photonView.RPC("CutNumber", RpcTarget.All, number);
        }
    }

    [PunRPC]
    void SetOpponentNumber(int index, int number)
    {
        Button btn = opponentButtons[index];
        opponentNumberMap[number] = btn;
    }

    [PunRPC]
    void NotifyPlacementDone()
    {
        opponentPlacementDone = true;
        CheckStartGame();
    }

    void CheckStartGame()
    {
        if (myPlacementDone && opponentPlacementDone)
        {
            placementPhase = false;
            isMyTurn = PhotonNetwork.IsMasterClient;
            instructionText.text = isMyTurn ? "Your turn: Cut a number!" : "Waiting for opponent...";
        }
    }

    [PunRPC]
    void CutNumber(int number)
    {
        // Disable number on your board
        if (myNumberMap.ContainsKey(number))
        {
            var btn = myNumberMap[number];
            btn.interactable = false;
            btn.GetComponentInChildren<Text>().color = Color.red;
        }

        // Show cut on opponent board
        if (opponentNumberMap.ContainsKey(number))
        {
            var btn = opponentNumberMap[number];
            btn.interactable = false;
            btn.GetComponentInChildren<Text>().text = "X";
            btn.GetComponentInChildren<Text>().color = Color.red;
        }

        instructionText.text = isMyTurn ? $"You cut {number}. Waiting..." : $"Opponent cut {number}. Your turn!";

        if (CheckForWin())
        {
            int winner = PhotonNetwork.IsMasterClient == isMyTurn ? 1 : 2;
            photonView.RPC("DeclareWinner", RpcTarget.All, winner);
        }
        else
        {
            isMyTurn = !isMyTurn;
        }
    }

    [PunRPC]
    void DeclareWinner(int winnerPlayer)
    {
        string result = (PhotonNetwork.IsMasterClient && winnerPlayer == 1) || (!PhotonNetwork.IsMasterClient && winnerPlayer == 2)
            ? "You Win!"
            : "You Lose!";
        instructionText.text = result;
        isMyTurn = false;
    }

    bool CheckForWin()
    {
        int lines = 0;

        // Rows
        for (int i = 0; i < 5; i++)
        {
            bool complete = true;
            for (int j = 0; j < 5; j++)
                if (myGrid[i, j].interactable)
                    complete = false;
            if (complete) lines++;
        }

        // Columns
        for (int j = 0; j < 5; j++)
        {
            bool complete = true;
            for (int i = 0; i < 5; i++)
                if (myGrid[i, j].interactable)
                    complete = false;
            if (complete) lines++;
        }

        // Diagonal
        bool diag1 = true, diag2 = true;
        for (int i = 0; i < 5; i++)
        {
            if (myGrid[i, i].interactable) diag1 = false;
            if (myGrid[i, 4 - i].interactable) diag2 = false;
        }
        if (diag1) lines++;
        if (diag2) lines++;

        return lines >= 5;
    }
    

}
