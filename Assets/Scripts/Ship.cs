using UnityEngine;

/// <summary>
/// Base class for all ships. Provides common settings and base movement logic.
/// </summary>
public class Ship : MonoBehaviour
{
	[Header("Movement settings")]
	[Tooltip("Approximate rotation speed value")]
	public float rotationSpeed = 70f;
	[Tooltip("Approximate move speed value")]
	public float moveSpeed = 500f;
	[Tooltip("Minimal distance in Units to start moving")]
	public float minDistance = 2.5f;
	[Tooltip("Minimal angle difference between look vector and destination vector to start moving")]
	public float minAngleDiffToApplyMovement = 3f;
	[Tooltip("Maximum angle difference between look vector and destination vector unlti which rotating is ignored")]
	public float maxAngleDiffToApplyRotation = 0.5f;

	[Header("Shooting settings")]
	public GameObject cannonballPrefab;
	[Tooltip("Approximate value of force that applied to cannonball when launching")]
	public float cannonballForceValue = 900f;

	protected Rigidbody _rb;

	private void Start()
	{
		_rb = GetComponent<Rigidbody>();
	}

    private void FixedUpdate()
    {
		FixForbiddenRotations();
    }

    private void FixForbiddenRotations()
    {
		var curRotation = transform.rotation.eulerAngles;

		curRotation.x = curRotation.z = 0;
		transform.eulerAngles = curRotation;
		_rb.angularVelocity = Vector3.zero;
	}

	/// <summary>
	/// Rotates towards given vector using trasform.rotate method.
	/// Takes in account angle between ship look vector and destination.
	/// </summary>
	/// <param name="destinationVec">Vector, towards which rotation is applying</param>
    protected void RotateTowards(Vector3 destinationVec)
    {
		var angleBetweenShipAndDestVec = Vector3.SignedAngle(transform.forward, destinationVec, transform.up);

		if (Mathf.Abs(angleBetweenShipAndDestVec) <= maxAngleDiffToApplyRotation)
			return;

		var multiplyingSign = angleBetweenShipAndDestVec > 0 ? 1f : -1f;

		transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime * multiplyingSign);
	}

	/// <summary>
	/// Moving towards given point using rigidbody.AddForce() method.
	/// Takes in account distance between ship center and target point, and angle between ship look vector and destination vector.
	/// </summary>
	/// <param name="targetPoint">Point toward which ship is gotta be moved</param>
	protected void MoveTowards(Vector3 targetPoint)
	{
		var destinationVec = targetPoint - transform.position;
		var angleBetweenShipAndDestVec = Vector3.SignedAngle(transform.forward, destinationVec, transform.up);

		if ((targetPoint - transform.position).magnitude <= minDistance)
			return;
		if (Mathf.Abs(angleBetweenShipAndDestVec) >= minAngleDiffToApplyMovement)
			return;

		_rb.AddForce(transform.forward * moveSpeed * Time.deltaTime);
	}

	/// <summary>
	/// Instantiates cannonball prefab by adding rigidbody.Addforce() method
	///		from given point through given shooting vector adding random amount of force in given range.
	/// </summary>
	/// <param name="shootingPoint">Point at which cannonball prefab instantiates.</param>
	/// <param name="shootingVec">Vector through which cannonball is being launched.</param>
	/// <param name="forceAdditionRange">Range determines random amount of force that is being applied.</param>
	protected void LaunchCannonball(Vector3 shootingPoint, Vector3 shootingVec, float forceAdditionRange = 0f)
    {
		var cannonball = Instantiate(cannonballPrefab, shootingPoint, Quaternion.identity);

		cannonball.GetComponent<Rigidbody>().AddForce(shootingVec.normalized * (cannonballForceValue + forceAdditionRange));
		cannonball.GetComponent<Rigidbody>().angularVelocity = new Vector3(Random.Range(0, 20), Random.Range(0, 20), Random.Range(0, 20));
	}
}
