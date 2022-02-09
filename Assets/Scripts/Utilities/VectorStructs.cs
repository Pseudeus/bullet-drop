using Godot;

public struct Vector3SByte
{
	public sbyte x;
	public sbyte y;
	public sbyte z;

	public Vector3SByte(sbyte x, sbyte y, sbyte z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
	public static Vector3SByte operator -(Vector3SByte left, Vector3SByte right)
	{
		sbyte xx = left.x;
		sbyte xy = left.y;
		sbyte xz = left.z;

		sbyte yx = right.x;
		sbyte yy = right.y;
		sbyte yz = right.z;

		return new Vector3SByte(
			checked((sbyte)(xx - yx)),
			checked((sbyte)(xy - yy)),
			checked((sbyte)(xz - yz)));
	}
	public static Vector3SByte operator +(Vector3SByte left, Vector3SByte right)
	{
		sbyte xx = left.x;
		sbyte xy = left.y;
		sbyte xz = left.z;

		sbyte yx = right.x;
		sbyte yy = right.y;
		sbyte yz = right.z;

		return new Vector3SByte(
			checked((sbyte)(xx + yx)),
			checked((sbyte)(xy + yy)),
			checked((sbyte)(xz + yz)));
	}
	public static Vector3SByte operator -(Vector3SByte x)
	{
		return new Vector3SByte((sbyte)-x.x, (sbyte)-x.y, (sbyte)-x.z);
	}
	public static implicit operator Vector3(Vector3SByte vector)
	{
		return new Vector3(vector.x, vector.y, vector.z);
	}
}

public struct Vector2Int
{
	public int x;
	public int y;

    public static Vector2Int Up => new Vector2Int(0, 1);
    public static Vector2Int Down => new Vector2Int(0, -1);
    public static Vector2Int Right => new Vector2Int(1, 0);
    public static Vector2Int Left => new Vector2Int(-1, 0);
	public static Vector2Int Zero => new Vector2Int(0, 0);
         

	public Vector2Int(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	public static Vector2Int operator -(Vector2Int left, Vector2Int right)
	{
		int xx = left.x;
		int xy = left.y;

		int yx = right.x;
		int yy = right.y;

		return new Vector2Int(
			checked(xx - yx),
			checked(xy - yy));
	}
	public static Vector2Int operator *(Vector2Int left, int right)
	{
		return new Vector2Int(left.x * right, left.y * right);
	}
	public static Vector2Int operator +(Vector2Int left, Vector2Int right)
	{
		int xx = left.x;
		int xy = left.y;

		int yx = right.x;
		int yy = right.y;

		return new Vector2Int(
			checked(xx + yx),
			checked(xy + yy));
	}
    public static bool operator ==(Vector2Int left, Vector2Int right)
    {
        if (left.x == right.x && left.y == right.y)
        {
            return true;
        }
        return false;
    }
    public static bool operator !=(Vector2Int left, Vector2Int right)
    {
        return !(left == right);
    }
	public static Vector2Int operator -(Vector2Int x)
	{
		return new Vector2Int(-x.x, -x.y);
	}
	public static implicit operator Vector2(Vector2Int vector)
	{
		return new Vector2(vector.x, vector.y);
	}

	public float LenghtSquared()
	{
		return (x * x) + (y * y);
	}
}
