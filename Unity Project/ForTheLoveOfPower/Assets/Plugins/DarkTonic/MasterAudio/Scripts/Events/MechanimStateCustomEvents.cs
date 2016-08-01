#if UNITY_5
using UnityEngine;

/*! \cond PRIVATE */
// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    // ReSharper disable once CheckNamespace
    public class MechanimStateCustomEvents : StateMachineBehaviour {
        [MasterCustomEvent]
        // ReSharper disable once InconsistentNaming
        public string enterCustomEvent;

        [MasterCustomEvent]
        // ReSharper disable once InconsistentNaming
        public string exitCustomEvent;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            MasterAudio.FireCustomEvent(enterCustomEvent, animator.transform.position);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            MasterAudio.FireCustomEvent(exitCustomEvent, animator.transform.position);
        }

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}
/*! \endcond */
#endif