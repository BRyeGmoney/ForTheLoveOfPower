using UnityEngine;
using System.Collections;

public class AlienMove : MonoBehaviour {

	[SerializeField] Vector3 m_TopLeftBound;
	[SerializeField] Vector3 m_BottomRightBound;
	[SerializeField] float m_MinMoveTime = 2f;
	[SerializeField] float m_MaxMoveTime = 5f;
	[SerializeField] float m_MinWaitTime = 0f;
	[SerializeField] float m_MaxWaitTime = 2f;
	[SerializeField] Animator m_Animator;

	private Transform m_Transform;
	private Vector3 m_Position;

	void Awake()
	{
		m_Transform = GetComponent<Transform>();
	}

	void Start()
	{
		m_Position = m_Transform.position;
		StartCoroutine(StartMoving());
	}

	private Vector3 GetRandomPosition()
	{
		return new Vector3(Random.Range(m_TopLeftBound.x, m_BottomRightBound.x)
		                   , Random.Range(m_TopLeftBound.y, m_BottomRightBound.y)
		                   , Random.Range(m_TopLeftBound.z, m_BottomRightBound.z));
	}

	private IEnumerator StartMoving()
	{
		while (true) {
			Vector3 temp = GetRandomPosition();

			m_Transform.localScale = new Vector3(temp.x >= m_Position.x ? 1f:-1f, 1f, 1f);
			m_Position = temp;
			yield return StartCoroutine(GoToPosition(Random.Range(m_MinMoveTime, m_MaxMoveTime)
			                                         , Random.Range(m_MinWaitTime, m_MaxWaitTime)
			                                         , m_Transform.position, m_Position));
		}
	}

	private IEnumerator GoToPosition(float time, float waitTime, Vector3 from, Vector3 to)
	{
		m_Animator.SetBool("IsWalking", true);

		float elapsedTime = 0f;
		
		while (elapsedTime < time) {
			m_Transform.position = Vector3.Lerp(from, to, elapsedTime/time); 
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		m_Animator.SetBool("IsWalking", false);
		yield return new WaitForSeconds(waitTime);
	}
}
