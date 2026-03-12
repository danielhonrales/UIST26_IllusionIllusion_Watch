package com.yourcompany.hapticwatch

import android.os.Bundle
import android.view.MotionEvent
import android.view.KeyEvent
import com.unity3d.player.UnityPlayer
import com.unity3d.player.UnityPlayerGameActivity

class BezelActivity : UnityPlayerGameActivity() {

    private var rotaryDelta = 0f
    private var totalAccumulated = 0f
    private val lock = Any()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.decorView.requestFocus()
    }

    override fun onResume() {
        super.onResume()
        window.decorView.requestFocus()
    }

    override fun onGenericMotionEvent(event: MotionEvent): Boolean {
        if (event.action == MotionEvent.ACTION_SCROLL) {
            val delta = event.getAxisValue(MotionEvent.AXIS_SCROLL)

            synchronized(lock) {
                rotaryDelta += delta
                totalAccumulated += delta
            }

            val direction = if (delta > 0) "cw" else "ccw"
            UnityPlayer.UnitySendMessage("BezelManager", "OnBezelEvent", "$direction:$delta")

            return true
        }
        return super.onGenericMotionEvent(event)
    }

    fun getRotaryDelta(): Float {
        synchronized(lock) {
            val value = rotaryDelta
            rotaryDelta = 0f
            return value
        }
    }

    fun getTotalAccumulated(): Float = synchronized(lock) { totalAccumulated }

    fun resetAccumulated() = synchronized(lock) { totalAccumulated = 0f }

    override fun onKeyDown(keyCode: Int, event: KeyEvent): Boolean {
    return when (keyCode) {
        KeyEvent.KEYCODE_BACK -> {
            UnityPlayer.UnitySendMessage("RoutineUI", "OnBackButton", "down")
            true // true = consumed, won't trigger default back behavior
        }
        KeyEvent.KEYCODE_STEM_1 -> {
            // Bottom home button (single press - may be intercepted by OS)
            UnityPlayer.UnitySendMessage("RoutineUI", "OnHomeButton", "down")
            true
        }
        else -> super.onKeyDown(keyCode, event)
    }
    }

    override fun onKeyUp(keyCode: Int, event: KeyEvent): Boolean {
        return when (keyCode) {
            KeyEvent.KEYCODE_BACK -> {
                UnityPlayer.UnitySendMessage("RoutineUI", "OnBackButton", "up")
                true
            }
            else -> super.onKeyUp(keyCode, event)
        }
    }   
}