using UnityEngine;
using UnityEngine.UI; // Image ���g���̂ŕK�v

public class Fadeout : MonoBehaviour
{
    private Animator anim = null;
    [SerializeField] private DoorWarp doorWarp; // Inspector�ŃA�T�C��
    private Image fadeImage; // �t�F�[�h�p��Image

    private void Start()
    {
        anim = GetComponent<Animator>();
        fadeImage = GetComponent<Image>();

        // �N������ Raycast Target ���I�t�ɂ��ă{�^��������ז����Ȃ�
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
        }
    }

    /// <summary>
    /// �t�F�[�h�A�E�g�J�n
    /// </summary>
    public void StartFadeout()
    {
        Debug.Log("�A�j���[�V�����X�^�[�g");

        // �t�F�[�h�J�n���͓��͂��u���b�N
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
        }

        anim.SetBool("fadeout", true);
    }

    /// <summary>
    /// �t�F�[�h�I�����ɃV�[����؂�ւ���
    /// Animator��AnimationEvent����Ă�
    /// </summary>
    public void Change()
    {
        if (doorWarp != null)
        {
            doorWarp.StartButton();
        }
        else
        {
            Debug.LogError("DoorWarp �� Inspector �Őݒ肳��Ă��܂���I");
        }
    }
}
