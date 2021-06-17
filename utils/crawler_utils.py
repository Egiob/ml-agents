import pandas as pd
import numpy as np

def parse_obs_126(x0):
    columns = ["pos_abs_x","pos_abs_y","pos_abs_z",
               "rot_abs_1","rot_abs_2","rot_abs_3", "rot_abs_4",
               "vel_abs_x","vel_abs_y","vel_abs_z",
               "rot_rel_1","rot_rel_2","rot_rel_3","rot_rel_4"]
    index = ['body', 'leg0','foreleg0', 'leg1','foreleg1', 'leg2','foreleg2', 'leg3','foreleg3']
    offset = 0
    size_0 = 27
    indices = np.arange(len(x0))
    x0_0 = x0[:size_0].reshape((9,3))
    x0_0_ind = indices[:size_0].reshape((9,3))

    offset += size_0
    size_1 = 36
    x0_1 = x0[offset:offset+size_1].reshape((9,4))
    x0_1_ind = indices[offset:offset+size_1].reshape((9,4))
    offset += size_1
    size_2 = 27
    x0_2 = x0[offset:offset+size_2].reshape((9,3))
    x0_2_ind = indices[offset:offset+size_2].reshape((9,3))
    offset += size_2
    size_3 = 36
    x0_3 = x0[offset:offset+size_3].reshape((9,4))
    x0_3_ind = indices[offset:offset+size_3].reshape((9,4))


    x0_df = pd.DataFrame(np.concatenate([x0_0,x0_1,x0_2,x0_3],axis=1),index=index,columns=columns)
    x0_df_ind = pd.DataFrame(np.concatenate([x0_0_ind,x0_1_ind,x0_2_ind,x0_3_ind],axis=1),index=index,columns=columns)
    
    return x0_df,x0_df_ind