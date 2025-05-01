// wwwroot/scripts/pitchviz.js

window.pitchViz = (() => {
    // Real‑world dimensions (in feet):
    const PLATE_WIDTH = 17 / 12;        // ≈1.417 ft (width of home plate)
    const MOUND_DIST = 60.5;          // Correct distance pitcher's rubber to back of plate
    const BALL_DIAMETER_INCHES = 2.9;   // Standard baseball diameter
    const BALL_RADIUS = (BALL_DIAMETER_INCHES / 2) / 12; // ≈0.12 ft radius
    const ZONE_THICK = 0.1;           // 0.1 ft thick strike‑zone plane visualization

    let scene, camera, renderer;
    let ball, trail, strikeZoneMesh;
    let animationId;

    /** Called once on first render */
    function initPitchViz(pbpJson) {
        const data = JSON.parse(pbpJson);
        const plays = data.AllPlays;
        if (!plays?.length) {
            console.warn("PitchViz: No plays found in JSON data.");
            return;
        }
        const lastPlay = plays[plays.length - 1];
        // Find the last event in the last play that IsPitch
        const pitchEv = lastPlay.PlayEvents.filter(e => e.IsPitch).slice(-1)[0];
        if (!pitchEv) {
            console.warn("PitchViz: No pitch event found in the last play.");
            return;
        }
        if (!pitchEv.PitchData?.Coordinates || !pitchEv.PitchData?.StrikeZoneTop) {
            console.error("PitchViz: PitchData or essential properties missing in the pitch event.", pitchEv);
            return;
        }

        // —— Renderer & Scene ——
        const canvas = document.getElementById('pitchCanvas');
        if (!canvas) {
            console.error("PitchViz: Canvas element with ID 'pitchCanvas' not found.");
            return;
        }
        renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
        renderer.setClearColor(0xe0f0e0); // Light green background
        // Ensure renderer size matches canvas display size
        renderer.setSize(canvas.clientWidth, canvas.clientHeight, false);

        scene = new THREE.Scene();
        camera = new THREE.PerspectiveCamera(
            30,                                     // Field of View (narrower = more zoom)
            canvas.clientWidth / canvas.clientHeight, // Aspect Ratio
            0.1,                                    // Near clipping plane
            200                                     // Far clipping plane (covers > 60.5 ft)
        );
        // Original position: 30 ft in front of plate, looking towards plate/mound
        camera.position.set(0, 2, 30);
        camera.lookAt(0, 2, 0); // Look at a point slightly above the plate center

        // Basic lighting
        scene.add(new THREE.AmbientLight(0xffffff, 0.7));
        const dirLight = new THREE.DirectionalLight(0xffffff, 0.9);
        dirLight.position.set(10, 15, 10); // Light from side/above
        scene.add(dirLight);

        // —— Baseball —— (Use correct radius)
        if (ball) {
            scene.remove(ball);
            ball.geometry.dispose();
            ball.material.dispose();
        }
        ball = new THREE.Mesh(
            new THREE.SphereGeometry(BALL_RADIUS, 16, 16), // Correct radius
            new THREE.MeshStandardMaterial({ color: 0xffffff }) // White ball
        );
        scene.add(ball);

        // —— Pitch Trail Line ——
        if (trail) {
            scene.remove(trail);
            trail.geometry.dispose();
            trail.material.dispose();
        }
        trail = new THREE.Line(
            new THREE.BufferGeometry(), // Geometry will be updated in animate()
            new THREE.LineBasicMaterial({ color: 0xff0000, linewidth: 2 }) // Red trail
            // Note: 'linewidth' > 1 might not work on all systems/browsers due to OpenGL limitations
        );
        scene.add(trail);

        // —— Strike Zone Box ——
        const top = pitchEv.PitchData.StrikeZoneTop;
        const bot = pitchEv.PitchData.StrikeZoneBottom;
        const height = Math.max(top - bot, 0.5); // Ensure minimum height
        if (strikeZoneMesh) {
            scene.remove(strikeZoneMesh);
            strikeZoneMesh.geometry.dispose();
            strikeZoneMesh.material.dispose();
        }
        strikeZoneMesh = new THREE.Mesh(
            new THREE.BoxGeometry(PLATE_WIDTH, height, ZONE_THICK),
            new THREE.MeshBasicMaterial({ color: 0x0000ff, transparent: true, opacity: 0.3 }) // Blue, semi-transparent
        );
        // Position strike zone vertically centered, at z=0 (plate location)
        strikeZoneMesh.position.set(0, (top + bot) / 2, 0);
        scene.add(strikeZoneMesh);

        // Handle window resizing
        window.addEventListener('resize', onWindowResize, false);

        // Kick off the animation for the first pitch
        updatePitch(pitchEv);
    }

    /** Handle window resize */
    function onWindowResize() {
        const canvas = renderer.domElement;
        camera.aspect = canvas.clientWidth / canvas.clientHeight;
        camera.updateProjectionMatrix();
        // Important: Set size again in case CSS changed canvas dimensions
        renderer.setSize(canvas.clientWidth, canvas.clientHeight, false);
        // No need to re-render here, animation loop will handle it
    }


    /** Called periodically (e.g., every 15 s) with fresh JSON */
    function updatePitchViz(pbpJson) {
        if (!scene) {
            console.warn("PitchViz: updatePitchViz called before initPitchViz completed.");
            return; // Don't try to update if initialization hasn't happened
        }
        const data = JSON.parse(pbpJson);
        const plays = data.AllPlays;
        if (!plays?.length) return; // No plays, nothing to update
        const lastPlay = plays[plays.length - 1];
        const pitchEv = lastPlay.PlayEvents.filter(e => e.IsPitch).slice(-1)[0];
        if (!pitchEv) return; // No new pitch event, nothing to update

        if (!pitchEv.PitchData?.Coordinates || !pitchEv.PitchData?.StrikeZoneTop) {
            console.error("PitchViz Update: PitchData or essential properties missing.", pitchEv);
            return;
        }

        // Stop any previous animation first
        if (animationId) cancelAnimationFrame(animationId);
        updatePitch(pitchEv); // Start animation with the new pitch data
    }

    /** Internal: setup & run the trajectory animation */
    function updatePitch(ev) {
        console.log("--- Updating Pitch Visualization ---");
        // Resize & reposition strike zone in case batter/heights changed
        const top = ev.PitchData.StrikeZoneTop;
        const bot = ev.PitchData.StrikeZoneBottom;
        const height = Math.max(top - bot, 0.5); // Use latest zone dimensions
        // Dispose old geometry before creating new one
        if (strikeZoneMesh) {
            strikeZoneMesh.geometry.dispose();
            strikeZoneMesh.geometry = new THREE.BoxGeometry(PLATE_WIDTH, height, ZONE_THICK);
            strikeZoneMesh.position.y = (top + bot) / 2; // Recenter vertically
        } else {
            console.warn("PitchViz Update: strikeZoneMesh not found during update.");
        }


        // --- DEBUGGING: Log Input Data ---
        const c = ev.PitchData.Coordinates;
        const tMax = ev.PitchData.PlateTime || 0.4; // Use provided time or default

        console.log("Input PitchData:", ev.PitchData);
        console.log("Using Coordinates (X0, Y0, Z0):", c.X0, c.Y0, c.Z0);
        console.log("Using Velocities (VX0, VY0, VZ0):", c.VX0, c.VY0, c.VZ0);
        console.log("Using Accelerations (AX, AY, AZ):", c.AX, c.AY, c.AZ);
        console.log("Using Plate Time (tMax):", tMax);
        // --- End Debugging Logs ---

        // Map MLB coords → Three.js (Y_mlb -> -Z_three, Z_mlb -> Y_three)
        // Assuming 1 unit = 1 foot
        // Note: Ensure your data's Y0 is distance from *plate* (~55ft), not mound (~5ft)
        const p0 = new THREE.Vector3(c.X0, c.Z0, -c.Y0); // Initial Position
        const v0 = new THREE.Vector3(c.VX0, c.VZ0, -c.VY0); // Initial Velocity (negate Y component)
        const acc = new THREE.Vector3(c.AX, c.AZ, -c.AY || 0); // Acceleration (negate Y component, handle missing AY)

        console.log("Calculated Initial State for Three.js:");
        console.log("  Start Position (p0):", `x: ${p0.x.toFixed(2)}, y: ${p0.y.toFixed(2)}, z: ${p0.z.toFixed(2)}`);
        console.log("  Start Velocity (v0):", `x: ${v0.x.toFixed(2)}, y: ${v0.y.toFixed(2)}, z: ${v0.z.toFixed(2)}`);
        console.log("  Acceleration (acc):", `x: ${acc.x.toFixed(2)}, y: ${acc.y.toFixed(2)}, z: ${acc.z.toFixed(2)}`);


        // Reset ball position and trail
        ball.position.copy(p0);
        let trailPoints = [p0.clone()]; // Start trail with the first point
        // Dispose old geometry and create new one, setting the first point
        trail.geometry.dispose();
        trail.geometry = new THREE.BufferGeometry().setFromPoints(trailPoints);

        const startTime = performance.now(); // Use performance.now() for higher precision timing

        function animate(now) { // 'now' is automatically passed by requestAnimationFrame
            animationId = requestAnimationFrame(animate); // Request next frame

            const t = (now - startTime) / 1000; // Time elapsed in seconds

            if (t <= tMax) {
                // Calculate current position using physics: p = p0 + v0*t + 0.5*a*t^2
                const currentPos = p0.clone()
                    .addScaledVector(v0, t) // Add v0 * t
                    .addScaledVector(acc, 0.5 * t * t); // Add 0.5 * a * t^2

                ball.position.copy(currentPos); // Update ball mesh position

                // Add point to trail if it's moved sufficiently
                // Check distance to prevent adding too many points very close together
                if (trailPoints.length === 0 || currentPos.distanceTo(trailPoints[trailPoints.length - 1]) > 0.05) { // Threshold distance = 0.05 ft
                    trailPoints.push(currentPos.clone()); // Add the new point

                    // Limit trail length (e.g., last 300 points) for performance
                    if (trailPoints.length > 300) {
                        trailPoints.shift(); // Remove oldest point
                    }

                    // Update the trail geometry with the new set of points
                    trail.geometry.setFromPoints(trailPoints);
                    trail.geometry.computeBoundingSphere(); // Helps Three.js know what to render
                }
            } else {
                // Animation finished, place ball exactly at final calculated position at tMax
                const finalPos = p0.clone()
                    .addScaledVector(v0, tMax)
                    .addScaledVector(acc, 0.5 * tMax * tMax);
                ball.position.copy(finalPos);

                // Add final point to trail if needed
                if (trailPoints.length === 0 || finalPos.distanceTo(trailPoints[trailPoints.length - 1]) > 0.01) {
                    trailPoints.push(finalPos.clone());
                    if (trailPoints.length > 300) trailPoints.shift();
                    trail.geometry.setFromPoints(trailPoints);
                    trail.geometry.computeBoundingSphere();
                }
                console.log("Animation finished at t =", t.toFixed(3), "s");
                cancelAnimationFrame(animationId); // Stop the loop
                animationId = null; // Clear the ID
            }

            // Render the scene in every frame
            renderer.render(scene, camera);
        }

        // Stop previous animation just in case (belt-and-suspenders)
        if (animationId) cancelAnimationFrame(animationId);
        // Start the new animation loop
        animate(performance.now()); // Pass initial time
    }

    // Public API
    return {
        initPitchViz,
        updatePitchViz
    };
})();