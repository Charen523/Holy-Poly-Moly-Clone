using UnityEngine;

public enum eMoveType
{
    Server,
    AddForce,
    Velocity
}

public class MiniTokenController
{
    private readonly MiniTokenData miniData;
    private readonly Transform transform;
    private readonly Rigidbody rb;

    public MiniTokenController(MiniTokenData data, Transform t, Rigidbody _rb)
    {
        miniData = data;
        transform = t;
        rb = _rb;
    }

    public void MoveVector2(eMoveType type)
    {
       switch (type)
        {
            case eMoveType.Server:
                transform.position = Vector3.MoveTowards(transform.position, miniData.nextPos, 30 * Time.deltaTime * Vector3.Distance(transform.position, miniData.nextPos));
                //시도해볼 보간법1.
                //transform.position = Vector3.Lerp(transform.position, nextPos, 0.1f * Time.deltaTime);
                //시도해볼 보간법2.
                //transform.position = Vector3.SmoothDamp(transform.position, nextPos, ref velocity, 0.2f);
                break;
            case eMoveType.AddForce:
                Vector3 force = new(miniData.wasdVector.x, 0, miniData.wasdVector.y);
                rb.AddForce(force * miniData.icePlayerSpeed, ForceMode.Force);
                break;
            case eMoveType.Velocity:
                break;
        }
    }

    public void RotateY()
    {
        transform.rotation = Quaternion.Euler(0f, miniData.rotY, 0f);
    }
}