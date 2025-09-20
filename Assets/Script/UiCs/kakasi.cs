using System.Collections;
using UnityEngine;

public class Kakasi : MonoBehaviour
{
    float kakasiHp = 110;

    public void TakekakasiDamage(float damage)
    {
        StartCoroutine(KakasiDamageFlash());
    }

    private IEnumerator KakasiDamageFlash()
    {
        Debug.Log("�J�J�V�����Ēɂ���");
        var sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f); // �Ԃ��Ȃ鎞��
        sr.color = Color.white;
    }

}
