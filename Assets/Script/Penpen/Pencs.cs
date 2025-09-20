using System.Collections;
using UnityEngine;

public class Pencs : MonoBehaviour
{
    float _penHp = 110;

    public void TakePenDamage(float damage)
    {
        _penHp -= damage;
        StartCoroutine(DamageFlash());
        Debug.Log("Pen‚Ìc‚èHP: " + _penHp);
        if (_penHp <= 0)
        {
            PenDie();
        }
    }
    private void PenDie()
    {
        Destroy(gameObject);
    }

    private IEnumerator DamageFlash()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f); // Ô‚­‚È‚éŠÔ
        sr.color = Color.white;
    }

}
