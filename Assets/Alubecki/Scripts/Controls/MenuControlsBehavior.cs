

public class MenuControlsBehavior : BaseControlsBehavior {


    protected override string CurrentActionMapName => "Menu";

    public bool MustGoBackToPreviousScreen { get; private set; }


    protected override void ResetControlsValues() {
        MustGoBackToPreviousScreen = false;
    }

    //callback from PlayerInput
    void OnBackToPreviousScreen() {
        MustGoBackToPreviousScreen = true;
    }

}
