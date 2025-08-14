import JSZip from 'jszip';

export interface ExtractedFile {
  name: string;
  content: Blob;
  size: number;
  type: string;
}

export interface ZipExtractionResult {
  success: boolean;
  files: ExtractedFile[];
  error?: string;
}

// Security constants to prevent zip bomb attacks
const MAX_TOTAL_SIZE = 100 * 1024 * 1024; // 100MB total extracted size
const MAX_FILE_COUNT = 100; // Maximum number of files in zip
const MAX_INDIVIDUAL_FILE_SIZE = 50 * 1024 * 1024; // 50MB per individual file
const MAX_COMPRESSION_RATIO = 100; // Maximum compression ratio (1:100)

/**
 * Extracts files from a zip file on the client side with security checks
 * @param zipFile - The zip file to extract
 * @returns Promise with extraction result
 */
export const extractZipFile = async (zipFile: File): Promise<ZipExtractionResult> => {
  try {
    const zip = new JSZip();
    const zipContent = await zip.loadAsync(zipFile);
    
    // Security check: File count limit
    const fileEntries = Object.values(zipContent.files).filter(entry => !entry.dir);
    if (fileEntries.length > MAX_FILE_COUNT) {
      return {
        success: false,
        files: [],
        error: `Too many files in zip archive. Maximum allowed: ${MAX_FILE_COUNT}, found: ${fileEntries.length}`
      };
    }
    
    // Security check: Compression ratio check
    const totalCompressedSize = zipFile.size;
    let totalUncompressedSize = 0;
    
    // Calculate total uncompressed size
    for (const entry of fileEntries) {
      totalUncompressedSize += entry._data.uncompressedSize || 0;
    }
    
    if (totalUncompressedSize > 0 && totalCompressedSize > 0) {
      const compressionRatio = totalUncompressedSize / totalCompressedSize;
      if (compressionRatio > MAX_COMPRESSION_RATIO) {
        return {
          success: false,
          files: [],
          error: `Suspicious compression ratio detected. This might be a zip bomb attack.`
        };
      }
    }
    
    const extractedFiles: ExtractedFile[] = [];
    let currentTotalSize = 0;
    
    // Process each file in the zip
    for (const [fileName, zipEntry] of Object.entries(zipContent.files)) {
      // Skip directories
      if (zipEntry.dir) continue;
      
      // Security check: Path traversal protection
      if (fileName.includes('..') || fileName.startsWith('/') || fileName.includes('\\')) {
        console.warn(`Skipping potentially malicious file path: ${fileName}`);
        continue;
      }
      
      // Security check: Individual file size limit
      const uncompressedSize = zipEntry._data.uncompressedSize || 0;
      if (uncompressedSize > MAX_INDIVIDUAL_FILE_SIZE) {
        console.warn(`Skipping oversized file: ${fileName} (${uncompressedSize} bytes)`);
        continue;
      }
      
      try {
        // Extract file content as blob
        const fileContent = await zipEntry.async('blob');
        
        // Security check: Total size limit
        if (currentTotalSize + fileContent.size > MAX_TOTAL_SIZE) {
          console.warn(`Total size limit reached, skipping remaining files`);
          break;
        }
        
        // Determine file type based on extension
        const fileExtension = fileName.toLowerCase().substring(fileName.lastIndexOf('.'));
        let mimeType = 'application/octet-stream';
        
        // Map common file extensions to MIME types
        const mimeTypeMap: Record<string, string> = {
          '.stl': 'application/octet-stream',
          '.obj': 'text/plain',
          '.fbx': 'application/octet-stream',
          '.gltf': 'model/gltf+json',
          '.glb': 'model/gltf-binary',
          '.3mf': 'model/3mf',
          '.step': 'application/octet-stream',
          '.stp': 'application/octet-stream',
          '.jpg': 'image/jpeg',
          '.jpeg': 'image/jpeg',
          '.png': 'image/png',
          '.gif': 'image/gif',
          '.webp': 'image/webp',
          '.bmp': 'image/bmp',
          '.md': 'text/markdown',
          '.markdown': 'text/markdown',
          '.txt': 'text/plain',
          '.pdf': 'application/pdf'
        };
        
        if (mimeTypeMap[fileExtension]) {
          mimeType = mimeTypeMap[fileExtension];
        }
        
        const extractedFile: ExtractedFile = {
          name: fileName,
          content: fileContent,
          size: fileContent.size,
          type: mimeType
        };
        
        extractedFiles.push(extractedFile);
        currentTotalSize += fileContent.size;
        
      } catch (fileError) {
        console.warn(`Failed to extract file ${fileName}:`, fileError);
        // Continue with other files
      }
    }
    
    if (extractedFiles.length === 0) {
      return {
        success: false,
        files: [],
        error: 'No valid files found in zip archive'
      };
    }
    
    return {
      success: true,
      files: extractedFiles
    };
    
  } catch (error) {
    console.error('Zip extraction failed:', error);
    return {
      success: false,
      files: [],
      error: error instanceof Error ? error.message : 'Unknown error during zip extraction'
    };
  }
};

/**
 * Checks if a file is a valid zip file by examining its header
 * @param file - The file to check
 * @returns Promise with boolean result
 */
export const isValidZipFile = async (file: File): Promise<boolean> => {
  try {
    // Check file size (zip files should be at least 22 bytes for minimal valid zip)
    if (file.size < 22) {
      console.warn('Zip file too small:', file.size, 'bytes');
      return false;
    }
    
    // Check file size upper limit to prevent extremely large zip files
    if (file.size > MAX_TOTAL_SIZE) {
      console.warn('Zip file too large:', file.size, 'bytes');
      return false;
    }
    
    // Read first 4 bytes to check zip signature
    const arrayBuffer = await file.slice(0, 4).arrayBuffer();
    const uint8Array = new Uint8Array(arrayBuffer);
    
    // ZIP file signature: PK\x03\x04 (0x50 0x4B 0x03 0x04)
    const isValidSignature = uint8Array[0] === 0x50 && 
                            uint8Array[1] === 0x4B && 
                            uint8Array[2] === 0x03 && 
                            uint8Array[3] === 0x04;
    
    if (!isValidSignature) {
      console.warn('Invalid zip file signature:', Array.from(uint8Array).map(b => b.toString(16).padStart(2, '0')).join(' '));
    }
    
    return isValidSignature;
  } catch (error) {
    console.error('Error validating zip file:', error);
    return false;
  }
};

/**
 * Converts extracted files to File objects for upload
 * @param extractedFiles - Array of extracted files
 * @returns Array of File objects
 */
export const convertToFiles = (extractedFiles: ExtractedFile[]): File[] => {
  return extractedFiles.map(extractedFile => {
    return new File([extractedFile.content], extractedFile.name, {
      type: extractedFile.type
    });
  });
};
