import React, { useState, useEffect } from 'react';
import { TextField, Button, Box, Select, MenuItem, InputLabel, FormControl } from '@mui/material';
import { LiveDocDocType } from '@/lib/schemas';

interface LiveDocFormProps {
  liveDoc?: LiveDocDocType | undefined;
  users: { id: string; name: string }[];
  workspaces: { id: string; name: string }[];
  onSubmit: (liveDoc: Omit<LiveDocDocType, 'id' | 'updatedAt' | 'isDeleted'>) => Promise<void>;
}

const LiveDocForm: React.FC<LiveDocFormProps> = ({ liveDoc, users, workspaces, onSubmit }) => {
  const [content, setContent] = useState('');
  const [ownerId, setOwnerId] = useState('');
  const [workspaceId, setWorkspaceId] = useState('');

  useEffect(() => {
    if (liveDoc) {
      setContent(liveDoc.content);
      setOwnerId(liveDoc.ownerId);
      setWorkspaceId(liveDoc.workspaceId);
    }
  }, [liveDoc]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    void onSubmit({ content, ownerId, workspaceId });
    if (!liveDoc) {
      setContent('');
      setOwnerId('');
      setWorkspaceId('');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField
          label="Content"
          multiline
          rows={4}
          value={content}
          onChange={(e) => setContent(e.target.value)}
          required
        />
        <FormControl fullWidth>
          <InputLabel>Owner</InputLabel>
          <Select value={ownerId} onChange={(e) => setOwnerId(e.target.value)} required>
            {users.map((user) => (
              <MenuItem key={user.id} value={user.id}>
                {user.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <FormControl fullWidth>
          <InputLabel>Workspace</InputLabel>
          <Select value={workspaceId} onChange={(e) => setWorkspaceId(e.target.value)} required>
            {workspaces.map((workspace) => (
              <MenuItem key={workspace.id} value={workspace.id}>
                {workspace.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Button type="submit" variant="contained">
          {liveDoc ? 'Update' : 'Create'} LiveDoc
        </Button>
      </Box>
    </form>
  );
};

export default LiveDocForm;
