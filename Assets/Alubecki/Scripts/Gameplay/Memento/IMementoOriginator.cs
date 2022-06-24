


public interface IMementoOriginator {

    IMementoSnapshot NewSnapshot();

    void Restore(IMementoSnapshot snapshot);

}
