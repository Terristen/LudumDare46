using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{

    public Image bar;
    public Bobtroller bob;
    public Tomtroller tom;
    public GameObject GameOverCanvas;

    // Start is called before the first frame update
    void Start()
    {
        if (bar == null)
            bar = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        bar.fillAmount = (bob != null) ? bob.Health / 100 : tom.Health / 100;

        bool gameOver = (bob != null) ? bob.Health <= 0 : tom.Health <= 0;

        if (gameOver)
        {
            GameOverCanvas.SetActive(true);
        }
    }
}
