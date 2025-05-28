using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BingoManager : MonoBehaviour
{
    public List<Button> bingoButtons; // Assign 25 buttons in Inspector
    public Text instructionText; // Assign a Text UI for instructions

    private int currentNumber = 1;
    private bool placementDone = false;
    private Dictionary<Button, bool> isPlaced = new Dictionary<Button, bool>();

    private Button[,] grid = new Button[5, 5];

    void Start()
    {
        instructionText.text = "Start placing numbers";

        // Build 5x5 grid
        for (int i = 0; i < bingoButtons.Count; i++)
        {
            isPlaced[bingoButtons[i]] = false;
            int row = i / 5;
            int col = i % 5;
            grid[row, col] = bingoButtons[i];
        }

        // Add listeners
        foreach (Button btn in bingoButtons)
        {
            btn.onClick.AddListener(() => OnButtonClick(btn));
        }
    }

    void OnButtonClick(Button btn)
    {
        Text btnText = btn.GetComponentInChildren<Text>();

        if (!placementDone && !isPlaced[btn])
        {
            btnText.text = currentNumber.ToString();
            isPlaced[btn] = true;
            currentNumber++;

            if (currentNumber <= 25)
                instructionText.text = $"Next number is {currentNumber}";
            else
            {
                placementDone = true;
                instructionText.text = "Check the number";
            }
        }
        else if (placementDone && btn.interactable)
        {
            btn.interactable = false;
            instructionText.text = $"Number {btnText.text} Cut";

            if (CheckForWin())
            {
                instructionText.text = " You win the game!";
            }
        }
    }

    bool CheckForWin()
    {
        int completedLines = 0;

        // Check Rows
        for (int row = 0; row < 5; row++)
        {
            int count = 0;
            for (int col = 0; col < 5; col++)
                if (!grid[row, col].interactable) count++;
            if (count == 5) completedLines++;
        }

        // Check Columns
        for (int col = 0; col < 5; col++)
        {
            int count = 0;
            for (int row = 0; row < 5; row++)
                if (!grid[row, col].interactable) count++;
            if (count == 5) completedLines++;
        }

        // Check Main Diagonal
        int diag1 = 0;
        for (int i = 0; i < 5; i++)
            if (!grid[i, i].interactable) diag1++;
        if (diag1 == 5) completedLines++;

        // Check Anti-Diagonal
        int diag2 = 0;
        for (int i = 0; i < 5; i++)
            if (!grid[i, 4 - i].interactable) diag2++;
        if (diag2 == 5) completedLines++;

        return completedLines >= 5;
    }

}
