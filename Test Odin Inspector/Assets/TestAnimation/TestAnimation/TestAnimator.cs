using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TestAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

	[Button]
	public void Test()
	{
		Animator animator = GetComponent<Animator>();
		animator.Update(0.1f);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
