using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PlayAnimationSample : MonoBehaviour
{
	public AnimationClip clip;
	private PlayableGraph playableGraph;

    void Start()
    {
		playableGraph = PlayableGraph.Create("PlayAnimationSample");
		playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
		var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
		var clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
		playableOutput.SetSourcePlayable(clipPlayable);
		playableGraph.Play();
	}

	void OnDisable()
	{
		playableGraph.Destroy();
	}
}
