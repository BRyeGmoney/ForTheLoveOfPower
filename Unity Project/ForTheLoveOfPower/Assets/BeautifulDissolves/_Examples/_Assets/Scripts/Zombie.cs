using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class Zombie : MonoBehaviour {

		[SerializeField] Animator m_Animator;
		[SerializeField] AudioSource m_Audio;
		[SerializeField] ParticleSystem m_DeathParticles;

		// Click zombie to kill
		void OnMouseDown()
		{
			GetComponent<Collider>().enabled = false;
			m_Animator.SetTrigger("Dead");
			m_Audio.Play();
			m_DeathParticles.Play();
		}

		public void DestroySelf()
		{
			Destroy(gameObject);
		}
	}
}
