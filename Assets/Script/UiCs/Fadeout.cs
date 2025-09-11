using UnityEngine;
using UnityEngine.UI; // Image を使うので必要

public class Fadeout : MonoBehaviour
{
    private Animator anim = null;
    [SerializeField] private DoorWarp doorWarp; // Inspectorでアサイン
    private Image fadeImage; // フェード用のImage

    private void Start()
    {
        anim = GetComponent<Animator>();
        fadeImage = GetComponent<Image>();

        // 起動時は Raycast Target をオフにしてボタン操作を邪魔しない
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
        }
    }

    /// <summary>
    /// フェードアウト開始
    /// </summary>
    public void StartFadeout()
    {
        Debug.Log("アニメーションスタート");

        // フェード開始時は入力をブロック
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
        }

        anim.SetBool("fadeout", true);
    }

    /// <summary>
    /// フェード終了時にシーンを切り替える
    /// AnimatorのAnimationEventから呼ぶ
    /// </summary>
    public void Change()
    {
        if (doorWarp != null)
        {
            doorWarp.StartButton();
        }
        else
        {
            Debug.LogError("DoorWarp が Inspector で設定されていません！");
        }
    }
}
