package com.yourcompany.hapticwatch

import android.os.Bundle
import android.view.MotionEvent
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
}