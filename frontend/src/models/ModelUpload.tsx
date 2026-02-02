import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import NavigationBar from '../components/common/NavigationBar';
import { useAppSelector, useAppDispatch } from '../store';
import { PrivacySettings } from '../api/client';
import ThumbnailGenerator from './ThumbnailGenerator';
import ModelViewer, { ViewMode } from './ModelViewer';
import PDFViewer from '../components/common/PDFViewer';
import MarkdownViewer from '../components/common/MarkdownViewer';
import { parseModelMarkdown, isMarkdownFile, generateMarkdownTemplate } from '../utils/markdownParser';
import { extractZipFile, isValidZipFile, convertToFiles } from '../utils/zipExtractor';
import { FileTypeSettingsData } from '../services/fileTypeSettingsService';
import { fetchFileSettings } from '../store/thunks/fileTypeSettingsThunks';
import modelConfigurationSettingsService, { ModelConfigurationSettings } from '../services/modelConfigurationSettingsService';

interface UploadedFile {
  id: string;
  name: string;
  size: number;
  type: string;
  file: File;
  progress: number;
  isThumbnail: boolean;
}

interface ModelData {
  title: string;
  description: string;
  privacy: PrivacySettings;
  license: string;
  categories: string[];
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
}

const MAX_FILES_PER_UPLOAD = 20;

const ModelUpload: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { user } = useAppSelector(state => state.auth);
  const { fileTypes: fileTypeSettings, isLoading: fileTypeSettingsLoading } = useAppSelector(state => state.fileTypeSettings);
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [modelData, setModelData] = useState<ModelData>({
    title: '',
    description: '',
    privacy: PrivacySettings.Public,
    license: 'MIT',
    categories: [],
    aiGenerated: false,
    workInProgress: false,
    nsfw: false,
    remix: false
  });
  const [previewFile, setPreviewFile] = useState<UploadedFile | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [autoRotate, setAutoRotate] = useState(false);
  const [showThumbnailGenerator, setShowThumbnailGenerator] = useState(false);
  const [thumbnailGeneratedMessage, setThumbnailGeneratedMessage] = useState<string | null>(null);
  const [isExtractingZip, setIsExtractingZip] = useState(false);
  const [modelConfigurationSettings, setModelConfigurationSettings] = useState<ModelConfigurationSettings | null>(null);
  const [modelConfigurationSettingsLoading, setModelConfigurationSettingsLoading] = useState(true);
  const [toast, setToast] = useState<{ open: boolean; message: string; severity: 'success' | 'error' | 'warning' | 'info' }>({
    open: false,
    message: '',
    severity: 'error'
  });
  
  const fileInputRef = useRef<HTMLInputElement>(null);

  const canAddMoreFiles = uploadedFiles.length < MAX_FILES_PER_UPLOAD;
  const remainingFiles = MAX_FILES_PER_UPLOAD - uploadedFiles.length;
  
  // Get supported formats from API settings
  const supported3DFormats = fileTypeSettings.filter((ft: FileTypeSettingsData) => ft.enabled && ft.category === '3D').map((ft: FileTypeSettingsData) => ft.fileExtension);
  const supportedImageFormats = fileTypeSettings.filter((ft: FileTypeSettingsData) => ft.enabled && ft.category === 'Image').map((ft: FileTypeSettingsData) => ft.fileExtension);
  const supportedDocumentFormats = fileTypeSettings.filter((ft: FileTypeSettingsData) => ft.enabled && ft.category === 'Document').map((ft: FileTypeSettingsData) => ft.fileExtension);
  const supportedZipFormats = fileTypeSettings.filter((ft: FileTypeSettingsData) => ft.enabled && ft.category === 'Archive').map((ft: FileTypeSettingsData) => ft.fileExtension);
  const allSupportedFormats = [...supported3DFormats, ...supportedImageFormats, ...supportedDocumentFormats, ...supportedZipFormats];

  const categories = [
    'Art', 'Technology', 'Toys', 'Tools', 'Games', 
    'Household', 'Engineering', 'Fashion', 'Medical', 'Other'
  ];

  const licenses = [
    'MIT', 'GPL', 'Creative Commons', 'Commercial', 'Custom'
  ];

  // Fetch file type settings from Redux store if not already loaded
  useEffect(() => {
    if (fileTypeSettings.length === 0 && !fileTypeSettingsLoading) {
      dispatch(fetchFileSettings());
    }
  }, [dispatch, fileTypeSettings.length, fileTypeSettingsLoading]);

  // Fetch model configuration settings on component mount
  useEffect(() => {
    const fetchModelConfigurationSettings = async () => {
      try {
        setModelConfigurationSettingsLoading(true);
        const response = await modelConfigurationSettingsService.getSettings();
        if (response.success && response.settings) {
          setModelConfigurationSettings(response.settings);
        }
      } catch (error) {
        console.error('Error fetching model configuration settings:', error);
      } finally {
        setModelConfigurationSettingsLoading(false);
      }
    };

    fetchModelConfigurationSettings();
  }, []);

  // Update model data when configuration settings are loaded
  useEffect(() => {
    if (modelConfigurationSettings) {
      setModelData(prev => ({
        ...prev,
        privacy: modelConfigurationSettings.defaultPrivacySetting as PrivacySettings
      }));
    }
  }, [modelConfigurationSettings]);

  // Process markdown file and populate model data
  const processMarkdownFile = async (file: File) => {
    try {
      const content = await file.text();
      const parsedData = parseModelMarkdown(content);
      
      // Update model data with parsed information
      setModelData(prev => ({
        ...prev,
        ...(parsedData.title && { title: parsedData.title }),
        ...(parsedData.description && { description: parsedData.description }),
        ...(parsedData.privacy && { privacy: parsedData.privacy }),
        ...(parsedData.license && { license: parsedData.license }),
        ...(parsedData.categories && { categories: parsedData.categories }),
        ...(parsedData.aiGenerated !== undefined && { aiGenerated: parsedData.aiGenerated }),
        ...(parsedData.workInProgress !== undefined && { workInProgress: parsedData.workInProgress }),
        ...(parsedData.nsfw !== undefined && { nsfw: parsedData.nsfw }),
        ...(parsedData.remix !== undefined && { remix: parsedData.remix })
      }));

      // Show confirmation message
      setThumbnailGeneratedMessage(`Markdown file processed! Model details have been populated from ${file.name}`);
      
      console.log('Processed markdown data:', parsedData);
    } catch (error) {
      console.error('Error processing markdown file:', error);
      alert('Error processing markdown file. Please check the file format.');
    }
  };

  // Process zip file and extract contents
  const processZipFile = async (file: File) => {
    try {
      if (!canAddMoreFiles) {
        setToast({
          open: true,
          message: `Maximum of ${MAX_FILES_PER_UPLOAD} files allowed per upload. Please remove some files before extracting this zip.`,
          severity: 'error'
        });
        return;
      }

      setIsExtractingZip(true);
      setThumbnailGeneratedMessage(`Extracting zip file: ${file.name}...`);
      
      // Validate zip file
      const isValidZip = await isValidZipFile(file);
      if (!isValidZip) {
        if (file.size < 22) {
          alert('Invalid zip file: File is too small to be a valid zip archive.');
        } else if (file.size > 1000 * 1024 * 1024) {
          alert('Invalid zip file: File is too large. Maximum size is 1000MB.');
        } else {
          alert('Invalid zip file: Please check that this is a valid zip archive.');
        }
        return;
      }

      // Extract zip contents
      const extractionResult = await extractZipFile(file);
      
      if (!extractionResult.success) {
        let errorMessage = 'Failed to extract zip file.';
        if (extractionResult.error) {
          if (extractionResult.error.includes('Too many files')) {
            errorMessage = 'Zip file contains too many files. Maximum allowed is 100 files.';
          } else if (extractionResult.error.includes('zip bomb')) {
            errorMessage = 'Security check failed: This zip file appears to be malicious.';
          } else {
            errorMessage = `Extraction failed: ${extractionResult.error}`;
          }
        }
        alert(errorMessage);
        return;
      }

      // Convert extracted files to File objects
      const extractedFiles = convertToFiles(extractionResult.files);
      
      // Limit files to maximum allowed
      const filesToAdd = extractedFiles.slice(0, remainingFiles);
      const filesSkipped = extractedFiles.length - filesToAdd.length;
      
      // Add extracted files to upload queue
      const newFiles: UploadedFile[] = filesToAdd.map(extFile => ({
        id: Math.random().toString(36).substr(2, 9),
        name: extFile.name,
        size: extFile.size,
        type: extFile.type,
        file: extFile,
        progress: 0,
        isThumbnail: false
      }));

      setUploadedFiles(prev => {
        const updated = [...prev, ...newFiles];
        if (filesSkipped > 0) {
          setToast({
            open: true,
            message: `Maximum of ${MAX_FILES_PER_UPLOAD} files reached. ${filesSkipped} file(s) from the zip were not added.`,
            severity: 'warning'
          });
        }
        return updated;
      });

      // Auto-preview first 3D model, image, PDF, or markdown found
      const firstPreviewableFile = newFiles.find(f => {
        const fileExtension = f.name.toLowerCase().substring(f.name.lastIndexOf('.'));
        return supported3DFormats.includes(fileExtension) || 
               supportedImageFormats.includes(fileExtension) ||
               f.name.toLowerCase().endsWith('.pdf') ||
               isMarkdownFile(f.name);
      });

      if (firstPreviewableFile) {
        setPreviewFile(firstPreviewableFile);
      }

      // Show success message with file count
      const modelFiles = newFiles.filter(f => {
        const fileExtension = f.name.toLowerCase().substring(f.name.lastIndexOf('.'));
        return supported3DFormats.includes(fileExtension);
      }).length;
      
      const imageFiles = newFiles.filter(f => {
        const fileExtension = f.name.toLowerCase().substring(f.name.lastIndexOf('.'));
        return supportedImageFormats.includes(fileExtension);
      }).length;
      
      const pdfFiles = newFiles.filter(f => f.name.toLowerCase().endsWith('.pdf')).length;
      const markdownFiles = newFiles.filter(f => isMarkdownFile(f.name)).length;
      const otherFiles = newFiles.length - modelFiles - imageFiles - pdfFiles - markdownFiles;
      
      let message = `Zip file extracted successfully! ${extractedFiles.length} files added to upload queue.`;
      if (modelFiles > 0) message += ` (${modelFiles} 3D models)`;
      if (imageFiles > 0) message += ` (${imageFiles} images)`;
      if (pdfFiles > 0) message += ` (${pdfFiles} PDFs)`;
      if (markdownFiles > 0) message += ` (${markdownFiles} markdown)`;
      if (otherFiles > 0) message += ` (${otherFiles} other files)`;
      
      setThumbnailGeneratedMessage(message);
      
      console.log('Extracted files from zip:', extractedFiles);
    } catch (error) {
      console.error('Error processing zip file:', error);
      let errorMessage = 'Error processing zip file.';
      if (error instanceof Error) {
        if (error.message.includes('JSZip')) {
          errorMessage = 'Failed to read zip file. The file may be corrupted or password protected.';
        } else {
          errorMessage = `Error: ${error.message}`;
        }
      }
      alert(errorMessage);
    } finally {
      setIsExtractingZip(false);
    }
  };

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (!files) return;

    if (!canAddMoreFiles) {
      setToast({
        open: true,
        message: `Maximum of ${MAX_FILES_PER_UPLOAD} files allowed per upload. Please remove some files before adding more.`,
        severity: 'error'
      });
      event.target.value = '';
      return;
    }

    const filesToAdd: File[] = [];
    const filesToProcess: File[] = [];

    for (const file of Array.from(files)) {
      if (!isFileAllowed(file)) {
        setToast({
          open: true,
          message: `File ${file.name} is not allowed or exceeds size limit.`,
          severity: 'error'
        });
        continue;
      }
      
      const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
      
      // Process zip files immediately
      if (supportedZipFormats.includes(fileExtension)) {
        filesToProcess.push(file);
        continue;
      }

      // Process markdown files immediately
      if (isMarkdownFile(file.name)) {
        filesToProcess.push(file);
        continue;
      }

      // Check if we can add this file
      if (uploadedFiles.length + filesToAdd.length >= MAX_FILES_PER_UPLOAD) {
        setToast({
          open: true,
          message: `Maximum of ${MAX_FILES_PER_UPLOAD} files allowed per upload. Only the first ${remainingFiles} files will be added.`,
          severity: 'warning'
        });
        break;
      }

      filesToAdd.push(file);
    }

    // Process zip and markdown files
    for (const file of filesToProcess) {
      const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
      if (supportedZipFormats.includes(fileExtension)) {
        await processZipFile(file);
      } else if (isMarkdownFile(file.name)) {
        await processMarkdownFile(file);
      }
    }

    // Add regular files
    const newFiles: UploadedFile[] = filesToAdd.map(file => {
      const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
      const newFile: UploadedFile = {
        id: Math.random().toString(36).substr(2, 9),
        name: file.name,
        size: file.size,
        type: file.type,
        file: file,
        progress: 0,
        isThumbnail: false
      };

      // Auto-preview 3D models, images, PDFs, and markdown files
      if (supported3DFormats.includes(fileExtension) || 
          supportedImageFormats.includes(fileExtension) ||
          file.name.toLowerCase().endsWith('.pdf') ||
          isMarkdownFile(file.name)) {
        if (!previewFile) {
          setPreviewFile(newFile);
        }
      }

      return newFile;
    });

    setUploadedFiles(prev => [...prev, ...newFiles]);
    event.target.value = '';
  };

  const handleDragOver = (event: React.DragEvent) => {
    event.preventDefault();
  };

  const handleDrop = async (event: React.DragEvent) => {
    event.preventDefault();
    
    if (!canAddMoreFiles) {
      setToast({
        open: true,
        message: `Maximum of ${MAX_FILES_PER_UPLOAD} files allowed per upload. Please remove some files before adding more.`,
        severity: 'error'
      });
      return;
    }

    const files = event.dataTransfer.files;
    
    if (files.length > 0) {
      const filesToAdd: File[] = [];
      const filesToProcess: File[] = [];

      for (const file of Array.from(files)) {
        if (!isFileAllowed(file)) {
          setToast({
            open: true,
            message: `File ${file.name} is not allowed or exceeds size limit.`,
            severity: 'error'
          });
          continue;
        }
        
        const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
        
        // Process zip files immediately
        if (supportedZipFormats.includes(fileExtension)) {
          filesToProcess.push(file);
          continue;
        }

        // Process markdown files immediately
        if (isMarkdownFile(file.name)) {
          filesToProcess.push(file);
          continue;
        }

        // Check if we can add this file
        if (uploadedFiles.length + filesToAdd.length >= MAX_FILES_PER_UPLOAD) {
          setToast({
            open: true,
            message: `Maximum of ${MAX_FILES_PER_UPLOAD} files allowed per upload. Only the first ${remainingFiles} files will be added.`,
            severity: 'warning'
          });
          break;
        }

        filesToAdd.push(file);
      }

      // Process zip and markdown files
      for (const file of filesToProcess) {
        const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
        if (supportedZipFormats.includes(fileExtension)) {
          await processZipFile(file);
        } else if (isMarkdownFile(file.name)) {
          await processMarkdownFile(file);
        }
      }

      // Add regular files
      const newFiles: UploadedFile[] = filesToAdd.map(file => {
        const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
        const newFile: UploadedFile = {
          id: Math.random().toString(36).substr(2, 9),
          name: file.name,
          size: file.size,
          type: file.type,
          file: file,
          progress: 0,
          isThumbnail: false
        };

        // Auto-preview 3D models, images, PDFs, and markdown files
        if (supported3DFormats.includes(fileExtension) || 
            supportedImageFormats.includes(fileExtension) ||
            file.name.toLowerCase().endsWith('.pdf') ||
            isMarkdownFile(file.name)) {
          if (!previewFile) {
            setPreviewFile(newFile);
          }
        }

        return newFile;
      });

      setUploadedFiles(prev => [...prev, ...newFiles]);
    }
  };



  const getFileType = (fileName: string) => {
    const fileExtension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'));
    if (supported3DFormats.includes(fileExtension)) return '3d';
    if (supportedImageFormats.includes(fileExtension)) return 'image';
    if (fileName.toLowerCase().endsWith('.pdf')) return 'pdf';
    if (isMarkdownFile(fileName)) return 'markdown';
    return 'unknown';
  };

  const getFileTypeSettings = (fileName: string) => {
    const fileExtension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'));
    return fileTypeSettings.find((ft: FileTypeSettingsData) => ft.enabled && ft.fileExtension.toLowerCase() === fileExtension);
  };

  const isFileAllowed = (file: File) => {
    const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
    const fileType = fileTypeSettings.find((ft: FileTypeSettingsData) => ft.enabled && ft.fileExtension.toLowerCase() === fileExtension);
    
    if (!fileType) return false;
    
    // Check file size
    if (file.size > fileType.maxFileSizeBytes) return false;
    
    return true;
  };

  const selectFileForPreview = (fileId: string) => {
    const file = uploadedFiles.find(f => f.id === fileId);
    if (file) {
      setPreviewFile(file);
    }
  };

  const removeFile = (fileId: string) => {
    setUploadedFiles(prev => {
      const filtered = prev.filter(file => file.id !== fileId);
      if (previewFile?.id === fileId) {
        setPreviewFile(filtered.length > 0 ? filtered[0] : null);
      }
      return filtered;
    });
  };

  const clearAllFiles = () => {
    setUploadedFiles([]);
    setPreviewFile(null);
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleInputChange = (field: keyof ModelData, value: any) => {
    setModelData(prev => ({ ...prev, [field]: value }));
  };

  const handleCategoryToggle = (category: string) => {
    setModelData(prev => ({
      ...prev,
      categories: prev.categories.includes(category)
        ? prev.categories.filter(c => c !== category)
        : [...prev.categories, category]
    }));
  };

  const handleThumbnailGenerated = (thumbnailBlob: Blob, fileName: string) => {
    const thumbnailFile = new File([thumbnailBlob], fileName, { type: 'image/png' });
    
    // Add the generated image to the upload queue (not automatically set as thumbnail)
    const newImageFile: UploadedFile = {
      id: Math.random().toString(36).substr(2, 9),
      name: fileName,
      size: thumbnailBlob.size,
      type: 'image/png',
      file: thumbnailFile,
      progress: 0,
      isThumbnail: false
    };

    setUploadedFiles(prev => [...prev, newImageFile]);
    
    setShowThumbnailGenerator(false);
    setThumbnailGeneratedMessage(`Custom image "${fileName}" added to upload queue`);
    
    // Clear the message after 3 seconds
    setTimeout(() => setThumbnailGeneratedMessage(null), 3000);
  };

  const handleUpload = async () => {
    if (uploadedFiles.length === 0) return;

    setIsUploading(true);
    setUploadProgress(0);

    try {
      // Prepare model data
      const thumbnailFile = uploadedFiles.find(f => f.isThumbnail);
      const modelUploadData = {
        name: modelData.title,
        description: modelData.description,
        privacy: modelData.privacy,
        license: modelData.license,
        categories: modelData.categories,
        aiGenerated: modelData.aiGenerated,
        workInProgress: modelData.workInProgress,
        nsfw: modelData.nsfw,
        remix: modelData.remix,
        thumbnailFileId: thumbnailFile?.name
      };

      // Prepare files - all files including thumbnails are already in uploadedFiles
      const files = uploadedFiles.map(f => f.file);

      // Import and use modelsService
      const { default: modelsService } = await import('../services/modelsService');
      
      const result = await modelsService.uploadModel({
        modelData: modelUploadData,
        files: files
      });
      
      console.log('Upload successful:', result);
      
      // Navigate to the newly created model's page
      if (result?.id) {
        navigate(`/models/${result.id}`);
      } else {
        // Fallback to dashboard if model ID is not available
        navigate('/dashboard');
      }

    } catch (error) {
      console.error('Upload failed:', error);
      const errorMessage = error instanceof Error ? error.message : 'Upload failed. Please try again.';
      setToast({
        open: true,
        message: errorMessage,
        severity: 'error'
      });
    } finally {
      setIsUploading(false);
      setUploadProgress(0);
    }
  };

  return (
    <div className="lg-container min-h-screen text-white flex flex-col">
      <style>{`
        .slider::-webkit-slider-thumb {
          appearance: none;
          height: 16px;
          width: 16px;
          border-radius: 50%;
          background: #10b981;
          cursor: pointer;
          border: none;
        }
        .slider::-moz-range-thumb {
          height: 16px;
          width: 16px;
          border-radius: 50%;
          background: #10b981;
          cursor: pointer;
          border: none;
        }
      `}</style>
      
      {/* Navigation Bar */}
      <NavigationBar
        title="Upload New Model"
        showSearch={false}
        showUploadButton={false}
        showHomeLink={true}
      />

      {/* Main Content - Padding for fixed navbar */}
      <div className="flex-1 pt-20">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex gap-8">
          {/* Left Panel - Upload Form */}
          <div className="flex-1">
            
            {/* Show drag and drop prominently when no files uploaded */}
            {fileTypeSettingsLoading || modelConfigurationSettingsLoading ? (
              <div className="mb-6">
                <div className="lg-card border-2 border-dashed border-gray-600 rounded-lg p-12 text-center">
                  <div className="lg-spinner w-8 h-8 mx-auto mb-4"></div>
                  <p className="text-gray-400">Loading file type and model settings...</p>
                </div>
              </div>
            ) : uploadedFiles.length === 0 ? (
              <div className="mb-6">
                <div
                  className="lg-card border-2 border-dashed border-gray-600 rounded-lg p-12 text-center hover:border-green-500 transition-colors"
                  onDragOver={handleDragOver}
                  onDrop={handleDrop}
                >
                  <svg className="mx-auto h-16 w-16 text-gray-400 mb-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                  </svg>
                  <p className="text-2xl text-gray-300 mb-3">Drag files to upload</p>
                  <p className="text-gray-500 mb-6">or</p>
                  <button
                    onClick={() => fileInputRef.current?.click()}
                    className="lg-button lg-button-primary text-lg px-8 py-3"
                  >
                    Choose Files
                  </button>
                  <input
                    ref={fileInputRef}
                    type="file"
                    multiple
                    accept={allSupportedFormats.join(',')}
                    onChange={handleFileSelect}
                    className="hidden"
                  />
                  <div className="mt-6 text-gray-400">
                    <p className="mb-1">Supported 3D formats: {supported3DFormats.join(', ')}</p>
                    <p className="mb-1">Supported image formats: {supportedImageFormats.join(', ')}</p>
                    <p className="mb-1">Supported document formats: {supportedDocumentFormats.join(', ')}</p>
                    <p className="mb-2">Supported archive formats: {supportedZipFormats.join(', ')}</p>
                    <div className="mb-3 p-2 bg-blue-900 border border-blue-600 rounded text-blue-200 text-xs">
                      <strong>Zip Security:</strong> Files are extracted client-side with protection against zip bombs, path traversal, and oversized files. Maximum: 100 files, 100MB total, 50MB per file.
                    </div>
                    <div className="mb-3 p-2 bg-green-900 border border-green-600 rounded text-green-200 text-xs">
                      <strong>💡 Tip:</strong> You can upload a zip file containing multiple 3D models, images, and documentation. All files will be automatically extracted and added to your upload queue. PDF and markdown files can be previewed directly in the browser. 3MF files are excellent for 3D printing as they preserve colors, materials, and multiple objects.
                    </div>
                    <button
                      onClick={() => {
                        const template = generateMarkdownTemplate();
                        const blob = new Blob([template], { type: 'text/markdown' });
                        const url = URL.createObjectURL(blob);
                        const a = document.createElement('a');
                        a.href = url;
                        a.download = 'model-template.md';
                        a.click();
                        URL.revokeObjectURL(url);
                      }}
                      className="text-green-400 hover:text-green-300 text-sm underline"
                    >
                      Download markdown template
                    </button>
                  </div>
                </div>
              </div>
            ) : (
              <>
                {/* Compact drag and drop area after files are uploaded */}
                <div className="mb-6">
                  <div
                    className={`lg-card border-2 border-dashed rounded-lg p-6 text-center transition-colors ${
                      canAddMoreFiles
                        ? 'border-gray-600 hover:border-green-500'
                        : 'border-gray-700 bg-gray-800/50 opacity-60 cursor-not-allowed'
                    }`}
                    onDragOver={canAddMoreFiles ? handleDragOver : undefined}
                    onDrop={canAddMoreFiles ? handleDrop : undefined}
                  >
                    <svg className={`mx-auto h-8 w-8 mb-3 ${canAddMoreFiles ? 'text-gray-400' : 'text-gray-600'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                    </svg>
                    <p className={`mb-2 ${canAddMoreFiles ? 'text-gray-300' : 'text-gray-500'}`}>
                      {canAddMoreFiles ? 'Add more files' : `Maximum files reached (${MAX_FILES_PER_UPLOAD})`}
                    </p>
                    <button
                      onClick={() => canAddMoreFiles && fileInputRef.current?.click()}
                      disabled={!canAddMoreFiles}
                      className={`lg-button lg-button-primary text-sm ${!canAddMoreFiles ? 'opacity-50 cursor-not-allowed' : ''}`}
                    >
                      Choose Files
                    </button>
                    <input
                      ref={fileInputRef}
                      type="file"
                      multiple
                      accept={allSupportedFormats.join(',')}
                      onChange={handleFileSelect}
                      disabled={!canAddMoreFiles}
                      className="hidden"
                    />
                  </div>
                </div>

                {/* Model Preview Area */}
                {previewFile ? (
                  <div className="mb-6">
                    <p className="text-sm text-gray-400 mb-2">
                      Previewing: {previewFile.name}
                    </p>
                    
                    {thumbnailGeneratedMessage && (
                      <div className="mb-4 p-3 bg-green-900 border border-green-600 rounded text-green-200 text-sm">
                        {isExtractingZip ? (
                          <div className="flex items-center">
                            <div className="lg-spinner w-4 h-4 mr-2"></div>
                            {thumbnailGeneratedMessage}
                          </div>
                        ) : (
                          `✓ ${thumbnailGeneratedMessage}`
                        )}
                      </div>
                    )}
                    <div className="lg-card rounded-lg overflow-hidden h-96 relative">
                      {getFileType(previewFile.name) === '3d' ? (
                        <ModelViewer
                          fileData={previewFile.file}
                          fileType={previewFile.name}
                          width="100%"
                          height={384}
                          autoRotate={autoRotate}
                          showControls={true}
                          isUploadMode={true}
                          modelFile={previewFile.file}
                          onShowThumbnailGenerator={() => setShowThumbnailGenerator(true)}
                          showFPS={true}
                          className="w-full h-full"
                        />
                      ) : getFileType(previewFile.name) === 'image' ? (
                        <div className="w-full h-full flex items-center justify-center">
                          <img 
                            src={URL.createObjectURL(previewFile.file)} 
                            alt="Preview" 
                            className="max-w-full max-h-full object-contain"
                          />
                        </div>
                      ) : getFileType(previewFile.name) === 'pdf' ? (
                        <PDFViewer
                          file={previewFile.file}
                          width="100%"
                          height={384}
                          className="w-full h-full"
                        />
                      ) : getFileType(previewFile.name) === 'markdown' ? (
                        <MarkdownViewer
                          file={previewFile.file}
                          width="100%"
                          height={384}
                          className="w-full h-full"
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center">
                          <div className="text-center">
                            <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                            </svg>
                            <p className="text-gray-400">Preview not available for this file type</p>
                          </div>
                        </div>
                      )}
                    </div>
                  </div>
                ) : (
                  <div className="mb-6">
                    <div className="lg-card rounded-lg overflow-hidden h-96 relative">
                      <div className="w-full h-full flex items-center justify-center">
                        <div className="text-center">
                          <svg className="mx-auto h-12 w-12 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                          </svg>
                          <p className="text-gray-400">Select a file from the upload queue to preview</p>
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </>
            )}

            {/* Model Information */}
            <div className="space-y-6">
              <div>
                <label className="block text-sm font-medium text-white mb-2">Title</label>
                <input
                  type="text"
                  value={modelData.title}
                  onChange={(e) => handleInputChange('title', e.target.value)}
                  className="lg-input"
                  placeholder="Enter model title"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-white mb-2">Description</label>
                <textarea
                  value={modelData.description}
                  onChange={(e) => handleInputChange('description', e.target.value)}
                  rows={4}
                  className="lg-input resize-none"
                  placeholder="Enter model description"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-white mb-2">Privacy</label>
                  <select
                    value={modelData.privacy}
                    onChange={(e) => handleInputChange('privacy', parseInt(e.target.value))}
                    className="lg-input"
                  >
                    <option value={PrivacySettings.Public}>Public - Everyone can see this model</option>
                    <option value={PrivacySettings.Private}>Private - Only you can see this model</option>
                    <option value={PrivacySettings.Unlisted}>Unlisted - Only people with the link can see this model</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-white mb-2">License</label>
                  <select
                    value={modelData.license}
                    onChange={(e) => handleInputChange('license', e.target.value)}
                    className="lg-input"
                  >
                    {licenses.map(license => (
                      <option key={license} value={license}>{license}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-white mb-2">Categories</label>
                <div className="flex flex-wrap gap-2">
                  {categories.map(category => (
                    <button
                      key={category}
                      onClick={() => handleCategoryToggle(category)}
                      className={`px-3 py-1 rounded-full text-sm border ${
                        modelData.categories.includes(category)
                          ? 'bg-green-600 border-green-500 text-white'
                          : 'bg-gray-800 border-gray-600 text-gray-300 hover:border-green-500'
                      }`}
                    >
                      {category}
                    </button>
                  ))}
                </div>
              </div>

              <div className="space-y-3">
                <label className="block text-sm font-medium text-white">Options</label>
                <div className="space-y-2">
                  {[
                    { key: 'aiGenerated', label: 'AI Generated' },
                    { key: 'workInProgress', label: 'Work in Progress' },
                    { key: 'nsfw', label: 'NSFW' },
                    { key: 'remix', label: 'Remix of Another Model' }
                  ].map(option => (
                    <label key={option.key} className="flex items-center">
                      <input
                        type="checkbox"
                        checked={modelData[option.key as keyof ModelData] as boolean}
                        onChange={(e) => handleInputChange(option.key as keyof ModelData, e.target.checked)}
                        className="mr-2 text-green-600 bg-gray-800 border-gray-600 rounded focus:ring-green-500"
                      />
                      <span className="text-sm text-gray-300">{option.label}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex justify-end space-x-4 pt-6">
                <button
                  onClick={() => navigate('/dashboard')}
                  className="lg-button"
                >
                  Cancel
                </button>
                <button
                  onClick={handleUpload}
                  disabled={isUploading || uploadedFiles.length === 0}
                  className="lg-button lg-button-primary disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isUploading ? (
                    <div className="flex items-center">
                      <div className="lg-spinner w-4 h-4 mr-2"></div>
                      Uploading...
                    </div>
                  ) : (
                    'Upload Model'
                  )}
                </button>
              </div>
            </div>
          </div>

          {/* Right Panel - Upload Queue */}
          <div className="w-80">
            <div className="lg-card p-4">
              <div className="flex justify-between items-center mb-4">
                <div>
                  <h3 className="text-lg font-medium text-green-400">Upload Queue</h3>
                  <p className="text-xs text-gray-400 mt-1">
                    {uploadedFiles.length} / {MAX_FILES_PER_UPLOAD} files
                    {!canAddMoreFiles && (
                      <span className="ml-2 text-yellow-400">(Maximum reached)</span>
                    )}
                  </p>
                </div>
                {uploadedFiles.length > 0 && (
                  <button
                    onClick={clearAllFiles}
                    className="text-red-400 hover:text-red-300 text-sm"
                  >
                    Clear All
                  </button>
                )}
              </div>
              
              {/* Help text for image selection */}
              {uploadedFiles.some(f => getFileType(f.name) === 'image') && (
                <div className="mb-4 p-3 bg-blue-900 border border-blue-600 rounded text-blue-200 text-sm">
                  <strong>Tip:</strong> Use the checkboxes next to images to select which one will be used as the model's thumbnail/preview image.
                </div>
              )}

              {uploadedFiles.length === 0 ? (
                <p className="text-gray-500 text-center py-8">No files selected</p>
              ) : (
                <div className="space-y-3">
                  {uploadedFiles.map(file => {
                    const isSelected = previewFile?.id === file.id;
                    const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
                    const is3DModel = supported3DFormats.includes(fileExtension);
                    const isImage = supportedImageFormats.includes(fileExtension);
                    const isPDF = file.name.toLowerCase().endsWith('.pdf');
                    const isMarkdown = isMarkdownFile(file.name);
                    
                    return (
                      <div 
                        key={file.id} 
                        className={`bg-gray-700 rounded p-3 cursor-pointer transition-all duration-200 hover:bg-gray-600 ${
                          isSelected ? 'ring-2 ring-green-500 bg-gray-600' : ''
                        }`}
                        onClick={() => selectFileForPreview(file.id)}
                      >
                        <div className="flex justify-between items-start mb-2">
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2">
                              {isImage && (
                                <input
                                  type="checkbox"
                                  checked={file.isThumbnail}
                                  onChange={(e) => {
                                    e.stopPropagation();
                                    if (e.target.checked) {
                                      // Set this image as thumbnail, uncheck others
                                      setUploadedFiles(prev => 
                                        prev.map(f => ({
                                          ...f,
                                          isThumbnail: f.id === file.id
                                        }))
                                      );
                                    } else {
                                      // Uncheck this image
                                      setUploadedFiles(prev => 
                                        prev.map(f => 
                                          f.id === file.id ? { ...f, isThumbnail: false } : f
                                        )
                                      );
                                    }
                                  }}
                                  className="mr-2 text-green-600 bg-gray-800 border-gray-600 rounded focus:ring-green-500"
                                  title="Set as thumbnail"
                                />
                              )}
                              <p className="text-sm text-white truncate">
                                {file.name}
                                {file.isThumbnail && (
                                  <span className="ml-2 text-xs text-green-400">Thumbnail</span>
                                )}
                              </p>
                              {is3DModel && (
                                <span className="text-xs text-blue-400">3D</span>
                              )}
                              {isImage && (
                                <span className="text-xs text-purple-400">Image</span>
                              )}
                              {isPDF && (
                                <span className="text-xs text-red-400">PDF</span>
                              )}
                              {isMarkdown && (
                                <span className="text-xs text-yellow-400">MD</span>
                              )}
                            </div>
                            <p className="text-xs text-gray-400">{formatFileSize(file.size)}</p>
                          </div>
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              removeFile(file.id);
                            }}
                            className="text-gray-400 hover:text-red-400 ml-2"
                          >
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                          </button>
                        </div>
                        <div className="w-full bg-gray-600 rounded-full h-1">
                          <div
                            className="bg-green-500 h-1 rounded-full transition-all duration-300"
                            style={{ width: `${file.progress}%` }}
                          />
                        </div>
                        <p className="text-xs text-gray-400 mt-1">{file.progress}% done</p>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
      
      {/* Thumbnail Generator Modal */}
      {showThumbnailGenerator && previewFile && (
        <ThumbnailGenerator
          modelFile={previewFile.file}
          onThumbnailGenerated={handleThumbnailGenerated}
          onCancel={() => setShowThumbnailGenerator(false)}
        />
      )}

      {/* Toast Notification */}
      {toast.open && (
        <div className={`fixed bottom-4 right-4 p-4 rounded-lg shadow-lg z-50 min-w-[300px] max-w-[500px] ${
          toast.severity === 'success' ? 'bg-green-600' :
          toast.severity === 'warning' ? 'bg-yellow-600' :
          toast.severity === 'info' ? 'bg-blue-600' :
          'bg-red-600'
        } text-white`}>
          <div className="flex items-center justify-between">
            <span className="flex-1 pr-4">{toast.message}</span>
            <button
              onClick={() => setToast(prev => ({ ...prev, open: false }))}
              className="text-white/80 hover:text-white font-bold text-lg leading-none"
              aria-label="Close"
            >
              ×
            </button>
          </div>
        </div>
      )}
      </div>
    </div>
  );
};

export default ModelUpload; 