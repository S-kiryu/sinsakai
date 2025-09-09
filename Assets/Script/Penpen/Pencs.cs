using UnityEngine;

public class Pencs : MonoBehaviour
{
    float _penHp = 110;

    public void TakePenDamage(float damage)
    {
        _penHp -= damage;
        Debug.Log("Pen‚ÌŽc‚èHP: " + _penHp);
        if (_penHp <= 0)
        {
            PenDie();
        }
    }
    private void PenDie()
    {
        Destroy(gameObject);
    }

}
