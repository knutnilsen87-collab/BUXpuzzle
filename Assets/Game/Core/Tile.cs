namespace Game.Core {
  public enum TileType { A,B,C,D,E,F }
  public enum TileState { Normal, Frozen, Locked, Blocker, Line, Burst, ColorBomb }
  public struct Tile {
    public TileType Type;
    public TileState State;
    public byte Fx;
  }
}
