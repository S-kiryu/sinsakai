using UnityEngine;

public class Pencs : MonoBehaviour
{
    float _penHp = 110;

    public void TakePenDamage(float damage)
    {
        _penHp -= damage;
        Debug.Log("Pen�̎c��HP: " + _penHp);
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
