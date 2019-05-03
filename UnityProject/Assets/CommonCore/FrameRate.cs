public class FrameRate {
	private int frameRate;
	private int numFrames = 0;
	private float polledTime = 0;
	
	public FrameRate () {
	}
	
	/**
	 * Update routines.
	 */
	public void Update(float timeElapsed) {
		++numFrames;
		polledTime += timeElapsed;
		
		if(Comparison.TolerantGreaterThanOrEquals(polledTime, 1.0f)) {
			// update frame rate
			frameRate = numFrames;
			
			// reset states
			numFrames = 0;
			polledTime = 0;
		}
	}
	
	/**
	 * Returns the frame rate.
	 */
	public int GetFrameRate() {
		return frameRate;
	}
}
