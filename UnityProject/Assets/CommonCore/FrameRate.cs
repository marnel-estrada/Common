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
		++this.numFrames;
		this.polledTime += timeElapsed;
		
		if(this.polledTime.TolerantGreaterThanOrEquals(1.0f)) {
			// update frame rate
			this.frameRate = this.numFrames;
			
			// reset states
			this.numFrames = 0;
			this.polledTime = 0;
		}
	}
	
	/**
	 * Returns the frame rate.
	 */
	public int GetFrameRate() {
		return this.frameRate;
	}
}
