using UnityEngine;
using UnityEngine.UI;
public class HPGauge : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] private float _maxHP = 100;
    [SerializeField] private float _currentHP = 100;
    [SerializeField] private float _changeValue = 20;

    private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //HPÇÃèâä˙âª
        _currentHP = _maxHP;
        GameObject playerObj = GameObject.FindWithTag("Player");
        player = playerObj.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        float hp = player.GetPlayerHp();
        _image.fillAmount = hp/100;
        Debug.Log("Player HP: " + hp);
    }
}
