using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class MixAnimationSample : MonoBehaviour
{
	public AnimationClip clip0;
	public AnimationClip clip1;
	public float weight;
	PlayableGraph playableGraph;
	AnimationMixerPlayable mixerPlayable;

    void Start()
    {
		playableGraph = PlayableGraph.Create("MixAnimationSample");
		var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
		mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);
		playableOutput.SetSourcePlayable(mixerPlayable);

		var clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);
		var clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);
		playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
		playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);

		playableGraph.Play();
	}

    void Update()
    {
		weight = Mathf.Clamp01(weight);
		mixerPlayable.SetInputWeight(0, 1f - weight);
		mixerPlayable.SetInputWeight(1, weight);
    }

	void OnDisable()
	{
		playableGraph.Destroy();
	}
}
