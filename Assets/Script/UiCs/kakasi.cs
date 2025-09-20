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
        Debug.Log("ÉJÉJÉVÇæÇ¡Çƒí…Ç¢ÇÒÇæ");
        var sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f); // ê‘Ç≠Ç»ÇÈéûä‘
        sr.color = Color.white;
    }

}
