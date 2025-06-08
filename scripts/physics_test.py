import numpy as np
import matplotlib.pyplot as plt

def update_physics(pos, vel, net_force):
    new_vel = vel + net_force
    new_pos = pos + new_vel
    return (new_pos, new_vel)

def run_physics(initial_pos, initial_vel, num_steps, pitch_history=0):
    pos = initial_pos
    vel = initial_vel
    pos_history = [pos]
    vel_history = [vel]
    force_history = []
    g = 0.08
    drag_coef = 0.75
    lift_coef = 0.1
    air_friction = 0.01
    pitch_history = np.ones(num_steps) * pitch_history
    glide_dir_history = np.array([np.cos(pitch_history), np.sin(pitch_history)]).T

    for _, glide_dir in zip(range(num_steps), glide_dir_history):
        # Equations adapted from: https://imgur.com/a/elytra-model-15w41b-Vwyjl
        gravity = np.array([0, -g])

        # speed_x = np.abs(vel[0])
        # glide = np.array([0.0, np.cos(pitch) ** 2 * g * 0.75])  # Acts against gravity
        # if vel[1] < 0 and np.cos(pitch) > 0:  # Player going down
        #     acc_y = vel[1] * -0.1 * np.cos(pitch) ** 2
        #     glide += np.array([acc_y, acc_y])
        # if pitch < 0:  # Player facing upward
        #     acc_y = speed_x * -np.sin(pitch) * 0.04
        #     glide += np.array([-acc_y, acc_y * 3.5])
        # if np.cos(pitch) > 0:
        #     glide += np.array([speed_x - vel[0], 0]) * 0.1
        # friction = air_friction * -vel  # Apply drag in direction opposite velocity
        # net_force = gravity + glide + friction

        glide = np.array([0.0, glide_dir[0] ** 2 * g * drag_coef])  # Acts against gravity
        if vel[1] < 0:  # Player going down
            glide += -lift_coef * glide_dir[0] ** 2 * vel[1] * np.array([np.sign(glide_dir[0]), 1])
        if glide_dir[1] > 0:  # Player facing upward
            glide += glide_dir[1] * vel[0] * np.array([-0.04, np.sign(vel[0]) * 0.14])
        friction = air_friction * -vel  # Apply drag in direction opposite velocity
        net_force = gravity + glide + friction

        pos, vel = update_physics(pos, vel, net_force)
        pos_history.append(pos)
        vel_history.append(vel)
        force_history.append(net_force)
    return np.array(pos_history), np.array(vel_history), np.array(force_history)

if __name__ == '__main__':
    initial_pos = np.array([0.0, 0.0])
    initial_vel = np.array([0.0, 0.0])
    # pitch_history = np.array([-55 * np.pi / 180] * 50 + [30 * np.pi / 180] * 100)
    pitch_history = np.array([-125 * np.pi / 180] * 50 + [150 * np.pi / 180] * 100)
    pos_history, vel_history, force_history = run_physics(initial_pos, initial_vel, len(pitch_history), pitch_history=pitch_history)

    fig, ((ax1, ax2), (ax3, ax4), (ax5, ax6)) = plt.subplots(3, 2)
    ax1.plot(pos_history[:, 0], pos_history[:, 1], marker='.', label='pos')
    ax1.set_xlabel('pos x')
    ax1.set_ylabel('pos y')
    ax1.axis('equal')
    ax1.legend()
    ax2.plot(pos_history[:, 0], marker='.', label='pos x')
    ax2.plot(pos_history[:, 1], marker='.', label='pos y')
    ax2.set_xlabel('time')
    ax2.legend()

    ax3.plot(vel_history[:, 0], vel_history[:, 1], marker='.', label='vel')
    ax3.set_xlabel('vel x')
    ax3.set_ylabel('vel y')
    ax3.axis('equal')
    ax3.legend()
    ax4.plot(vel_history[:, 0], marker='.', label='vel x')
    ax4.plot(vel_history[:, 1], marker='.', label='vel y')
    ax4.set_xlabel('time')
    ax4.legend()

    ax5.plot(force_history[:, 0], force_history[:, 1], marker='.', label='force')
    ax5.set_xlabel('force x')
    ax5.set_ylabel('force y')
    ax5.axis('equal')
    ax5.legend()
    ax6.plot(force_history[:, 0], marker='.', label='force x')
    ax6.plot(force_history[:, 1], marker='.', label='force y')
    ax6.set_xlabel('time')
    ax6.legend()

    for ax in (ax1, ax2, ax3, ax4, ax5, ax6):
        ax.grid(True, which='both')
        ax.axhline(y=0, color='k')
        ax.axvline(x=0, color='k')
    plt.show()