using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class RuntimeControllerSample : MonoBehaviour
{
	public AnimationClip clip;
	public RuntimeAnimatorController controller;
	public float weight;
	PlayableGraph playableGraph;
	AnimationMixerPlayable mixerPlayable;

	// Start is called before the first frame update
	void Start()
    {
		playableGraph = PlayableGraph.Create("MixAnimationSample");
		var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
		mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);
		playableOutput.SetSourcePlayable(mixerPlayable);

		var clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip);
		var clipPlayable1 = AnimatorControllerPlayable.Create(playableGraph, controller);
		playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
		playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);

		playableGraph.Play();
	}

    // Update is called once per frame
    void Update()
    {
		weight = Mathf.Clamp01(weight);
		mixerPlayable.SetInputWeight(0, 1f - weight);
		mixerPlayable.SetInputWeight(1, 1f);
	}

	void OnDisable()
	{
		playableGraph.Destroy();
	}
}
