package gltron.org.gltronmobile;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

public class MainActivity extends Activity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        try {
            Intent intent = new Intent();
            intent.setClassName("gltron.org.gltronmobile", "gltron.org.gltronmobile.Activity1");
            startActivity(intent);
        } catch (Exception ex) {
            android.util.Log.e("GLTRON", "Failed to launch MonoGame Activity1 from Java stub", ex);
        } finally {
            finish();
        }
    }
}
