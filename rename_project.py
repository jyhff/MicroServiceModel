#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import os
import re

def preserve_case_replace(text, old_pattern, new_pattern):
    if not text:
        return text
    
    old_lower = old_pattern.lower()
    old_original = old_pattern
    new_original = new_pattern
    
    def replace_match(match):
        matched = match.group(0)
        if matched == old_original:
            return new_original
        elif matched == old_lower:
            return new_original.lower()
        elif matched == old_original.upper():
            return new_original.upper()
        elif matched[0].isupper():
            return new_original[0].upper() + new_original[1:]
        else:
            return new_original.lower()
    
    pattern = re.compile(re.escape(old_original), re.IGNORECASE)
    return pattern.sub(replace_match, text)

def replace_in_file(file_path):
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except (UnicodeDecodeError, PermissionError):
        return False
    
    # Apply multiple replacements
    new_content = content
    replacements = [
        ('LCH.MicroService', 'LCH.MicroService'),
        ('LCH.Abp', 'LCH.Abp'),
        ('LCH', 'LCH'),
    ]
    for old_pattern, new_pattern in replacements:
        new_content = preserve_case_replace(new_content, old_pattern, new_pattern)
    
    if new_content != content:
        try:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(new_content)
            print(f"Updated content: {file_path}")
            return True
        except PermissionError:
            print(f"Permission denied: {file_path}")
            return False
    return False

def rename_items(root_dir):
    renamed_count = 0
    
    replacements = [
        ('LCH.MicroService', 'LCH.MicroService'),
        ('LCH.Abp', 'LCH.Abp'),
        ('LCH', 'LCH'),
    ]
    
    for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
        for filename in filenames:
            old_path = os.path.join(dirpath, filename)
            new_filename = filename
            for old_pattern, new_pattern in replacements:
                new_filename = preserve_case_replace(new_filename, old_pattern, new_pattern)
            if new_filename != filename:
                new_path = os.path.join(dirpath, new_filename)
                try:
                    os.rename(old_path, new_path)
                    print(f"Renamed file: {filename} -> {new_filename}")
                    renamed_count += 1
                except OSError as e:
                    print(f"Error renaming file {filename}: {e}")
        
        for dirname in dirnames:
            old_path = os.path.join(dirpath, dirname)
            new_dirname = dirname
            for old_pattern, new_pattern in replacements:
                new_dirname = preserve_case_replace(new_dirname, old_pattern, new_pattern)
            if new_dirname != dirname:
                new_path = os.path.join(dirpath, new_dirname)
                try:
                    os.rename(old_path, new_path)
                    print(f"Renamed folder: {dirname} -> {new_dirname}")
                    renamed_count += 1
                except OSError as e:
                    print(f"Error renaming folder {dirname}: {e}")
    
    return renamed_count

def process_directory(root_dir):
    exclude_dirs = {'.git', '.vs', 'bin', 'obj', 'node_modules', '.idea', '.vscode'}
    exclude_extensions = {'.pyc', '.pyo', '.dll', '.exe', '.png', '.jpg', '.jpeg', '.gif', '.ico', '.woff', '.woff2', '.ttf', '.eot', '.pdf', '.zip', '.nupkg', '.snupkg'}
    
    content_updated = 0
    files_processed = 0
    
    for dirpath, dirnames, filenames in os.walk(root_dir):
        dirnames[:] = [d for d in dirnames if d not in exclude_dirs]
        
        for filename in filenames:
            ext = os.path.splitext(filename)[1].lower()
            if ext in exclude_extensions:
                continue
            
            file_path = os.path.join(dirpath, filename)
            files_processed += 1
            
            if replace_in_file(file_path):
                content_updated += 1
    
    print(f"\nFiles processed: {files_processed}")
    print(f"Files with content updated: {content_updated}")
    
    renamed = rename_items(root_dir)
    print(f"Items renamed: {renamed}")

if __name__ == '__main__':
    script_path = os.path.abspath(__file__)
    root_dir = os.path.dirname(script_path)
    
    print(f"Starting replacement in: {root_dir}")
    print("Replacing 'LCH.MicroService' -> 'LCH.MicroService' (preserving case)")
    print("Replacing 'LCH.Abp' -> 'LCH.Abp' (preserving case)")
    print("Replacing 'LCH' -> 'LCH' (preserving case)")
    print("-" * 50)
    
    process_directory(root_dir)
    
    print("-" * 50)
    print("Done!")