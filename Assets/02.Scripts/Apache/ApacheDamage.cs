using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApacheDamage : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer[] renderers;
    private readonly string tankTag = "TANK";
    private readonly string apacheTag = "APACHE";
    private GameObject expEffect = null;
    private readonly int initHp = 100;
    private int curHp = 0;
    public Image hpBar;
    public Canvas tankCanvas;
    private WaitForSeconds ws;

    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
