using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    public int TestInt;
    public float TestFloat;
    public string TestString;
    public bool TestBool;
    public List<int> TestList;
    public List<string> TestListString;
    public List<float> TestListFloat;
    public List<bool> TestListBool;

    private void Start()
    {
        Debug.Log("Hi");
    }
}
