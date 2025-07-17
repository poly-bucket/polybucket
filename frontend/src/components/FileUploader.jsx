import React, { useState, useRef } from 'react';
import axios from 'axios';
import {
  Box,
  Button,
  Text,
  Flex,
  Input,
  Progress,
  List,
  ListItem,
  IconButton,
  Link,
  useToast,
  VStack,
  HStack,
  Heading
} from '@chakra-ui/react';
import { FiUpload, FiDownload, FiTrash, FiCheck, FiX } from 'react-icons/fi';

const FileUploader = () => {
  const [files, setFiles] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const fileInputRef = useRef(null);
  const toast = useToast();

  const handleFileChange = (event) => {
    const fileList = event.target.files;
    if (fileList && fileList.length > 0) {
      handleUpload(fileList[0]);
    }
  };

  const handleUpload = async (file) => {
    setUploading(true);
    setUploadProgress(0);
    
    try {
      const formData = new FormData();
      formData.append('file', file);
      
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      const response = await axios.post(`${baseUrl}/api/FileStorage/upload`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress: (progressEvent) => {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total
          );
          setUploadProgress(percentCompleted);
        },
      });
      
      if (response.data && response.data.fileName) {
        setFiles(prevFiles => [...prevFiles, {
          name: file.name,
          fileName: response.data.fileName,
          url: response.data.url,
          size: file.size,
          type: file.type,
          uploadDate: new Date()
        }]);
        
        toast({
          title: "File uploaded",
          description: `${file.name} has been uploaded successfully.`,
          status: "success",
          duration: 5000,
          isClosable: true,
        });
      }
    } catch (error) {
      console.error('Error uploading file:', error);
      toast({
        title: "Upload failed",
        description: error.response?.data || "An error occurred during upload.",
        status: "error",
        duration: 5000,
        isClosable: true,
      });
    } finally {
      setUploading(false);
      setUploadProgress(0);
      // Reset file input
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const handleDelete = async (fileName) => {
    try {
      const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:11666';
      await axios.delete(`${baseUrl}/api/FileStorage/${fileName}`);
      
      setFiles(prevFiles => prevFiles.filter(file => file.fileName !== fileName));
      
      toast({
        title: "File deleted",
        description: "File has been deleted successfully.",
        status: "success",
        duration: 5000,
        isClosable: true,
      });
    } catch (error) {
      console.error('Error deleting file:', error);
      toast({
        title: "Delete failed",
        description: error.response?.data || "An error occurred during deletion.",
        status: "error",
        duration: 5000,
        isClosable: true,
      });
    }
  };

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatDate = (date) => {
    return new Date(date).toLocaleString();
  };

  return (
    <Box p={4} borderWidth="1px" borderRadius="lg" w="100%">
      <VStack spacing={4} align="stretch">
        <Heading size="md">File Storage</Heading>
        
        <Box>
          <Input
            type="file"
            ref={fileInputRef}
            onChange={handleFileChange}
            display="none"
            id="file-upload"
          />
          <Button
            as="label"
            htmlFor="file-upload"
            colorScheme="blue"
            leftIcon={<FiUpload />}
            isDisabled={uploading}
            cursor="pointer"
            w="100%"
          >
            {uploading ? 'Uploading...' : 'Select File to Upload'}
          </Button>
          
          {uploading && (
            <Box mt={2}>
              <Progress value={uploadProgress} size="sm" colorScheme="blue" />
              <Text mt={1} fontSize="sm" textAlign="center">
                {uploadProgress}%
              </Text>
            </Box>
          )}
        </Box>
        
        {files.length > 0 && (
          <Box>
            <Text fontWeight="bold" mb={2}>Uploaded Files</Text>
            <List spacing={2}>
              {files.map((file, index) => (
                <ListItem
                  key={index}
                  p={3}
                  borderWidth="1px"
                  borderRadius="md"
                  bg="gray.50"
                >
                  <Flex justifyContent="space-between" alignItems="center">
                    <VStack align="start" spacing={1}>
                      <Text fontWeight="bold">{file.name}</Text>
                      <Text fontSize="sm" color="gray.500">
                        {formatFileSize(file.size)} • {formatDate(file.uploadDate)}
                      </Text>
                    </VStack>
                    
                    <HStack>
                      <Link href={`${import.meta.env.VITE_API_URL || 'http://localhost:11666'}/api/FileStorage/${file.fileName}`} isExternal>
                        <IconButton
                          icon={<FiDownload />}
                          aria-label="Download file"
                          size="sm"
                          colorScheme="green"
                        />
                      </Link>
                      <IconButton
                        icon={<FiTrash />}
                        aria-label="Delete file"
                        size="sm"
                        colorScheme="red"
                        onClick={() => handleDelete(file.fileName)}
                      />
                    </HStack>
                  </Flex>
                </ListItem>
              ))}
            </List>
          </Box>
        )}
        
        {files.length === 0 && !uploading && (
          <Box textAlign="center" p={4} color="gray.500">
            <Text>No files uploaded yet</Text>
          </Box>
        )}
      </VStack>
    </Box>
  );
};

export default FileUploader; 