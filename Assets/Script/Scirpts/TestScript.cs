using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{

[Header("기본 헤더 테스트")]
public bool Test1;

[Header("헤더 중첩 테스트")]
public bool Test2;


private void Start()
{
    Test1 = true;
    Debug.Log("Hi");
}
}
