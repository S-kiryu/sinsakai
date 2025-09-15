using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [TextArea]
    [SerializeField] private string[] lines; // 会話のセリフを並べる
    private int currentLine = 0;

    private void Start()
    {
        ShowLine();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // スペースで次へ
        {
            NextLine();
        }
    }

    private void ShowLine()
    {
        if (currentLine < lines.Length)
        {
            dialogueText.text = lines[currentLine];
        }
    }

    private void NextLine()
    {
        currentLine++;
        if (currentLine < lines.Length)
        {
            ShowLine();
        }
        else
        {
            gameObject.SetActive(false); // 終了したら非表示
        }
    }
}