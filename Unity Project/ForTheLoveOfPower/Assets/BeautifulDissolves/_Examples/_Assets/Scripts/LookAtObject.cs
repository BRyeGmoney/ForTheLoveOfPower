using UnityEngine;
using System.Collections;

namespace BeautifulDissolves {
	public class LookAtObject : MonoBehaviour {

		public Transform target;
		private Transform m_Transform;

		void Awake()
		{
			m_Transform = GetComponent<Transform>();
		}

		void Update ()
		{
			m_Transform.LookAt(target);
		}
	}
}
