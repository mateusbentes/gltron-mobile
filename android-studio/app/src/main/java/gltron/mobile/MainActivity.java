package gltron.mobile;

import android.app.Activity;
import android.os.Bundle;

public class MainActivity extends Activity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Best-effort placeholder: runtime wiring is not available yet.
        android.util.Log.e("GLTRON", "MonoGame runtime not wired. Cannot launch C# Game.");
        // TODO: Replace with MonoGame Android bootstrap when runtime is available.
        finish();
    }
}
