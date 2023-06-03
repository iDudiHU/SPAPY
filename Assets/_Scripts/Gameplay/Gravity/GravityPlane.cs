using UnityEngine;

public class GravityPlane : GravitySource {

    [SerializeField]
    float gravity = 9.81f;

    [SerializeField, Min(0f)]
    float range = 1f;
	[SerializeField]
	float width = 1f;
	[SerializeField]
	float length = 1f;


	public override Vector3 GetGravity(Vector3 position)
	{
		Vector3 up = transform.up;
		Vector3 displacement = position - transform.position;
		float distance = Vector3.Dot(up, displacement);
		Vector3 localPosition = transform.InverseTransformPoint(position);

		if (Mathf.Abs(localPosition.x) > width / 2 || Mathf.Abs(localPosition.z) > length / 2) {
			return Vector3.zero;
		}

		if (distance > range || distance < 0f) {
			return Vector3.zero;
		}

		float g = -gravity;
		if (distance > 0f) {
			g *= 1f - distance / range;
		}
		return g * up;
	}
	void OnDrawGizmos()
	{
		Vector3 scale = new Vector3(width, range, length);
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);

		Vector3 size = new Vector3(1f, 0f, 1f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(Vector3.zero, size);
		if (range > 0f) {
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(Vector3.up, size);
		}
	}
}