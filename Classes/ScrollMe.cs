namespace CraftKill.Classes;

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ScrollMe : MonoBehaviour
{
    public MeshRenderer mr;

    public void Start()
    {
        mr = GetComponent<MeshRenderer>();
    }

    public void Update()
    {
        mr.material.SetTextureOffset("_MainTex", mr.material.GetTextureOffset("_MainTex") + Vector2.right * 0.2f * Time.deltaTime);
    }
}
