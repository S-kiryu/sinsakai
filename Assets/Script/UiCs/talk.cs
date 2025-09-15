using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [TextArea]
    [SerializeField] private string[] lines; // ��b�̃Z���t����ׂ�
    private int currentLine = 0;

    private void Start()
    {
        ShowLine();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // �X�y�[�X�Ŏ���
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
            gameObject.SetActive(false); // �I���������\��
        }
    }
}